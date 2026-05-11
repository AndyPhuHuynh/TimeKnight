using System;
using TimeKnight.Core.Player;
using TimeKnight.Core.TimePower;
using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.Enemy
{
    [Serializable]
    public struct FloatRange
    {
        public float Min;
        public float Max;

        public FloatRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    public enum JumpEnemyState
    {
        Idle,
        PatrolJumpWindup,
        PatrolJump,
        ChaseJumpWindup,
        ChaseJump,
        Falling,
    }
    public class JumpEnemyMovement : MonoBehaviour
    {
        private Rigidbody2D _rb = null!;

        private JumpEnemyState _currentState = JumpEnemyState.Idle;
        private Vector3 _playerPosition => PlayerController.PlayerPosition;

        // External Control From enemy-specific script.
        private bool _isAiPaused = false;
        public event Action<JumpEnemyState> OnEnemyStateEnter = delegate { };
        public event Action<JumpEnemyState> OnEnemyStateExit = delegate { };

        [Header("Jump Properties")]
        [SerializeField] private FloatRange verticalJumpForceRange = new FloatRange(2f, 5f);
        [SerializeField] private FloatRange horizontalJumpForceRange = new FloatRange(2f, 5f);
        private float _jumpTimer;
        private float _minimumAirtime = 0.5f;
        private float _airtimeTimer;

        [Header("Patrol Properties")]
        [SerializeField] private float patrolJumpCooldown = 4f;
        [SerializeField] private float patrolRange = 10f;
        [SerializeField] private Vector3 wallCheckOffset = new Vector3(1f, 0f, 0f);
        [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f, 1f);
        private bool _readyToPatrolJump => _jumpTimer >= patrolJumpCooldown;
        private Vector3 _patrolAnchor;
        private float _teleportReturnTime = 10f;
        private float _timeAwayFromPatrol;

        [Header("Chase Properties")]
        [SerializeField] private float chaseRange = 10f;
        [SerializeField] private float chaseJumpCooldown = 2f;
        private bool _readyToChaseJump => _jumpTimer >= chaseJumpCooldown;
        private bool _isPlayerInChaseable;

        // Directional/Scaling
        private bool _isFacingLeft => transform.localScale.x < 0;
        private int _directionModifier => _isFacingLeft ? -1 : 1;
        private float _baseTransformXScale;

        // Collision
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private GroundCheck groundCheck = null!;


        private void OnValidate()
        {
            Validation.NotNull(this, groundCheck, nameof(groundCheck));
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _patrolAnchor = transform.position;
            _baseTransformXScale = Math.Abs(transform.localScale.x);
        }

        private void Start()
        {
            OnEnemyStateEnter.Invoke(_currentState);
        }

        public void Update()
        {
            if (_isAiPaused) return;

            _isPlayerInChaseable = CheckIsPlayerChaseable();

            switch (_currentState)
            {
                case JumpEnemyState.Idle:
                    OnIdle();
                    break;
                case JumpEnemyState.ChaseJump:
                    OnChaseJump();
                    break;
                case JumpEnemyState.PatrolJump:
                    OnPatrolJump();
                    break;
                case JumpEnemyState.Falling:
                    OnFalling();
                    break;
            }
        }

        private void OnIdle()
        {
            _jumpTimer += TimeManager.CustomDelta;

            if (_isPlayerInChaseable)
            {
                FacePlayer();
                if (_readyToChaseJump)
                {
                    TransitionTo(JumpEnemyState.ChaseJumpWindup);
                }
                return;
            }

            if (IsOutOfPatrolRange())
            {
                _timeAwayFromPatrol += TimeManager.CustomDelta;
            }
            else
            {
                _timeAwayFromPatrol = 0;
            }

            if (_timeAwayFromPatrol >= _teleportReturnTime)
            {
                _rb.position = _patrolAnchor;
                return;
            }

            if (_readyToPatrolJump)
            {
                if (IsMovingAwayFromPatrolRange()) FlipEnemyDirection();
                TransitionTo(JumpEnemyState.PatrolJumpWindup);
            }
        }

        private void OnChaseJump()
        {
            _rb.AddForce(GetChaseJumpForce(), ForceMode2D.Impulse);

            TransitionTo(JumpEnemyState.Falling);
        }

        private void OnPatrolJump()
        {
            _rb.AddForce(GetPatrolJumpForce(), ForceMode2D.Impulse);

            TransitionTo(JumpEnemyState.Falling);
        }

        public void OnJumpWindupComplete()
        {
            switch (_currentState)
            {
                case JumpEnemyState.PatrolJumpWindup:
                    TransitionTo(JumpEnemyState.PatrolJump);
                    break;
                case JumpEnemyState.ChaseJumpWindup:
                    TransitionTo(JumpEnemyState.ChaseJump);
                    break;
            }
        }
        private void OnFalling()
        {
            if (groundCheck.IsGrounded && _airtimeTimer >= _minimumAirtime)
            {
                TransitionTo(JumpEnemyState.Idle);
                return;
            }
            _airtimeTimer += TimeManager.CustomDelta;
        }

        private void TransitionTo(JumpEnemyState newState)
        {
            if (_currentState == newState) return;

            switch (_currentState)
            {
                case JumpEnemyState.Idle:
                    _jumpTimer = 0;
                    break;
            }

            OnEnemyStateExit.Invoke(_currentState);

            switch (newState)
            {
                case JumpEnemyState.Idle:
                    _jumpTimer = 0;
                    break;
                case JumpEnemyState.Falling:
                    _airtimeTimer = 0;
                    break;
                case JumpEnemyState.ChaseJumpWindup:
                case JumpEnemyState.PatrolJumpWindup:
                    if (IsHittingWall())
                    {
                        FlipEnemyDirection();
                    }
                    break;
            }

            OnEnemyStateEnter.Invoke(newState);

            _currentState = newState;
        }

        private Vector2 GetPatrolJumpForce()
        {
            float xForce = UnityEngine.Random.Range(horizontalJumpForceRange.Min, horizontalJumpForceRange.Max) * TimeManager.CurrentTimeModifier * _directionModifier;
            float yForce = UnityEngine.Random.Range(verticalJumpForceRange.Min, verticalJumpForceRange.Max) * TimeManager.CurrentTimeModifier;
            return new Vector2(xForce, yForce);
        }

        // TODO: shift the jump to be a bit more horizontal again
        private Vector2 GetChaseJumpForce()
        {
            // Estimate horizontal force based on distance to player, clamped to chase range.
            float horizontalDistance = _playerPosition.x - transform.position.x;
            float absHoriz = Mathf.Abs(horizontalDistance);
            float horizRatio = Mathf.Clamp01(absHoriz / chaseRange);

            // Favor a more horizontal arc for chase jumps by increasing horizontal magnitude
            // and reducing the upward bias compared to earlier behavior.
            float xMag = Mathf.Lerp(horizontalJumpForceRange.Min, horizontalJumpForceRange.Max * 1.05f, horizRatio);
            float xForce = xMag * TimeManager.CurrentTimeModifier * Mathf.Sign(horizontalDistance);

            // Reduce vertical bias so enemy jumps flatter when chasing. Use vertical distance
            // to slightly increase height when the player is significantly above.
            float verticalDistance = _playerPosition.y - transform.position.y;
            float vertRatio = Mathf.Clamp01(verticalDistance / chaseRange);
            float yMag = Mathf.Lerp(verticalJumpForceRange.Min * 1.05f, verticalJumpForceRange.Max * 1.35f, vertRatio);
            float yForce = yMag * TimeManager.CurrentTimeModifier;

            return new Vector2(xForce, yForce);
        }

        private bool IsMovingAwayFromPatrolRange()
        {
            // To determine if enemy is moving away from patrol range, we use signed horizontal distance because enemy cannot jump and direction matters here; so we can't use Euclidean distance.
            float horizontalDistance = GetSignedHorizontalDistanceTo(_patrolAnchor);

            if (!_isFacingLeft && horizontalDistance > patrolRange)
            {
                // If enemy moving right and has gone past patrol range in positive x, enemy moving out of range.
                return true;
            }
            else if (_isFacingLeft && horizontalDistance < patrolRange * -1)
            {
                // If enemy moving left, and we went past range in negative x, enemy moving out of range.
                return true;
            }

            return false;
        }

        private void FacePlayer()
        {
            bool isPlayerToRight = _playerPosition.x > transform.position.x;

            if (isPlayerToRight)
            {
                FaceRight();
            }
            else
            {
                FaceLeft();
            }
        }
        private void FlipEnemyDirection()
        {
            if (_isFacingLeft)
            {
                FaceRight();
            }
            else
            {
                FaceLeft();
            }
        }

        private void FaceRight()
        {
            Vector3 scale = transform.localScale;
            scale.x = _baseTransformXScale;
            transform.localScale = scale;
        }

        private void FaceLeft()
        {
            Vector3 scale = transform.localScale;
            scale.x = -_baseTransformXScale;
            transform.localScale = scale;
        }

        private float GetSignedHorizontalDistanceTo(Vector3 other) => transform.position.x - other.x;
        private float GetEuclideanDistanceTo(Vector2 other) => Vector2.Distance(transform.position, other);
        private bool IsOutOfPatrolRange() => GetEuclideanDistanceTo(_patrolAnchor) > patrolRange;

        private bool CheckIsPlayerChaseable()
        {
            // Only perform raycast if player is within chaseable range.
            if (GetEuclideanDistanceTo(_playerPosition) > chaseRange) return false;

            Vector2 enemyPosition = transform.position;
            Vector2 playerPosition = _playerPosition;
            Vector2 directionToPlayer = playerPosition - enemyPosition;
            float distanceToPlayer = directionToPlayer.magnitude;

            RaycastHit2D hit = Physics2D.Raycast(
                enemyPosition,
                directionToPlayer / distanceToPlayer,
                distanceToPlayer,
                groundLayer | playerLayer
            );

            // Only returns true if a collision was made and it was on the player layer.
            return hit.collider != null && ((1 << hit.collider.gameObject.layer) & playerLayer) != 0;
        }
        private Vector2 GetWallCheckPosition() => transform.position + new Vector3(wallCheckOffset.x * _directionModifier, wallCheckOffset.y, wallCheckOffset.z);
        private bool IsHittingWall()
        {
            Vector2 wallCheckPosition = GetWallCheckPosition();
            bool isHittingWall = Physics2D.OverlapBox(wallCheckPosition, wallCheckSize, 0f, groundLayer);
            return isHittingWall;
        }
        private void OnDrawGizmosSelected()
        {
            Vector2 wallCheckPosition = GetWallCheckPosition();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(wallCheckPosition, wallCheckSize);
        }
    }
}
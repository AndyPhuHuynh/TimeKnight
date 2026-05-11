using System;
using TimeKnight.Core.Player;
using TimeKnight.Core.TimePower;
using UnityEngine;

namespace TimeKnight.Core.Enemy
{
    public enum PatrolEnemyState
    {
        Patrol,
        Chase,
        LostSight,
        TooClose,
        PatrolStuck,
        ChaseStuck
    }

    public class PatrolEnemyMovement : MonoBehaviour
    {
        private Rigidbody2D _rb = null!;

        private PatrolEnemyState _currentState = PatrolEnemyState.Patrol;
        private Vector3 _playerPosition => PlayerController.PlayerPosition;

        // AI Management from enemy script. Actions allow enemy script to react when enemy state changes.
        private bool _isAiPaused = false;
        public event Action<PatrolEnemyState> OnEnemyStateExit = delegate { };
        public event Action<PatrolEnemyState> OnEnemyStateEnter = delegate { };

        [Header("Patrol Properties")]
        [SerializeField] private float patrolWalkSpeed = 3f;
        [SerializeField] private float patrolRange = 10f;
        private Vector3 _patrolAnchor;
        private float _teleportReturnTime = 10f;
        private float _timeAwayFromPatrol = 0f;

        [Header("Chase Properties")]
        [SerializeField] private float chaseRange = 5f;
        [SerializeField] private float chaseWalkSpeed = 4f;
        [SerializeField] private float minimumPlayerDistance = 0.2f;
        public bool IsPlayerChaseable { get; private set; }

        [Header("Lost Sight Properties")]
        [SerializeField] private float lostSightPatrolCooldown = 5f;
        private float _lostSightTimer = 0f;

        [Header("Collision Checking")]
        [SerializeField] private Vector3 wallCheckOffset = new Vector3(1f, 0f, 0f);
        [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f, 1f);
        [SerializeField] private Vector3 ledgeCheckOffset = new Vector3(1f, 0f, 0f);
        [SerializeField] private Vector2 ledgeCheckSize = new Vector2(0.5f, 1f);
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask playerLayer;

        // Directional variables
        private bool _isFacingLeft => transform.localScale.x < 0;
        private int _directionModifier => _isFacingLeft ? -1 : 1;
        private float _baseTransformXScale;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _patrolAnchor = transform.position;
            _baseTransformXScale = Math.Abs(transform.localScale.x);
        }

        private void Start()
        {
            // This is called in Start deliberately to give time for OnEnable subscriptions to activate.
            OnEnemyStateEnter.Invoke(_currentState);
        }

        # region AI Logic / State Management
        public void Update()
        {
            if (_isAiPaused) return;

            IsPlayerChaseable = CheckIsPlayerChaseable();

            switch (_currentState)
            {
                case PatrolEnemyState.Patrol:
                    Patrol();
                    break;
                case PatrolEnemyState.PatrolStuck:
                    PatrolStuck();
                    break;
                case PatrolEnemyState.Chase:
                    Chase();
                    break;
                case PatrolEnemyState.ChaseStuck:
                    ChaseStuck();
                    break;
                case PatrolEnemyState.LostSight:
                    LostSight();
                    break;
                case PatrolEnemyState.TooClose:
                    TooClose();
                    break;
            }
        }

        private void Patrol()
        {
            if (IsPlayerChaseable)
            {
                TransitionTo(PatrolEnemyState.Chase);
                return;
            }

            // IsHittingWall and IsNearLedge are not cached because those values might be changed after calling FlipPatrolDirection.
            bool _isMovingOutOfPatrolRange = IsMovingAwayFromPatrolRange();
            bool _isOutOfPatrolRange = IsOutOfPatrolRange();

            // Patrolling Has 3 Core steps
            // 1. Make sure enemy is moving towards patrol zone, OR if they are in patrol zone - flip when hit wall or ledge
            // 2. While walking back to patrol zone, teleport back if timer is up.
            // 3. If enemy gets stuck walking back, transition to PatrolStuck, and wait for teleport.
            // Finally, apply movement

            // 1.
            if (_isMovingOutOfPatrolRange || !_isOutOfPatrolRange && (IsHittingWall() || IsNearLedge()))
            {
                FlipEnemyDirection();
            }

            // 2.
            if (_isOutOfPatrolRange)
            {
                _timeAwayFromPatrol += TimeManager.CustomDelta;
            }
            else
            {
                _timeAwayFromPatrol = 0f;
            }

            if (_timeAwayFromPatrol >= _teleportReturnTime)
            {
                transform.position = _patrolAnchor;
            }

            // 3.
            if (_isOutOfPatrolRange && (IsHittingWall() || IsNearLedge()))
            {
                TransitionTo(PatrolEnemyState.PatrolStuck);
                return;
            }

            _rb.linearVelocityX = patrolWalkSpeed * GetVelocityModifiers();
        }

        private void PatrolStuck()
        {
            if (_timeAwayFromPatrol >= _teleportReturnTime)
            {
                transform.position = _patrolAnchor;
                TransitionTo(PatrolEnemyState.Patrol);
                return;
            }
            else if (IsPlayerChaseable)
            {
                TransitionTo(PatrolEnemyState.Chase);
                return;
            }

            _rb.linearVelocityX = 0;
            _timeAwayFromPatrol += TimeManager.CustomDelta;
        }

        private void Chase()
        {
            if (!IsPlayerChaseable)
            {
                TransitionTo(PatrolEnemyState.LostSight);
                return;
            }
            // If enemy is too close to the player, move into too close state
            else if (GetSignedHorizontalDistanceTo(_playerPosition) <= minimumPlayerDistance)
            {
                TransitionTo(PatrolEnemyState.TooClose);
                return;
            }
            else if (IsNearLedge() || IsHittingWall())
            {
                TransitionTo(PatrolEnemyState.ChaseStuck);
                return;  
            }

            FacePlayer();
            _rb.linearVelocityX = chaseWalkSpeed * GetVelocityModifiers();
        }

        private void ChaseStuck()
        {
            if (!IsPlayerChaseable)
            {
                TransitionTo(PatrolEnemyState.LostSight);
                return;
            }
            else if (!IsNearLedge() && !IsHittingWall())
            {
                TransitionTo(PatrolEnemyState.Chase);
                return;
            }

            FacePlayer();
            _rb.linearVelocityX = 0;
        }

        private void LostSight()
        {
            // Chase player if sight is returned, but otherwise wait for cooldown before returning to patrol.
            if (IsPlayerChaseable)
            {
                TransitionTo(PatrolEnemyState.Chase);
                return;
            }
            else if (_lostSightTimer >= lostSightPatrolCooldown)
            {
                TransitionTo(PatrolEnemyState.Patrol);
                return;
            }

            _rb.linearVelocityX = 0;
            _lostSightTimer += TimeManager.CustomDelta;
        }

        private void TooClose()
        {
            // Don't transition back to chasing until the player is a bit past minium distance.
            // This prevents freaking out when player is at edge of minimum distance 
            if (GetSignedHorizontalDistanceTo(_playerPosition) > minimumPlayerDistance + 0.5)
            {
                TransitionTo(PatrolEnemyState.Chase);
                return;
            }
            else if (!IsPlayerChaseable)
            {
                TransitionTo(PatrolEnemyState.LostSight);
                return;
            }

            FacePlayer();
            _rb.linearVelocityX = 0;
        }

        private void TransitionTo(PatrolEnemyState newState)
        {
            if (_currentState == newState) return;

            OnEnemyStateExit.Invoke(_currentState);

            // Perform actions needed for entering new state, then enter new state;
            switch (newState)
            {
                case PatrolEnemyState.Patrol: _timeAwayFromPatrol = 0f; break;
                case PatrolEnemyState.LostSight: _lostSightTimer = 0f; break;
                case PatrolEnemyState.Chase:
                case PatrolEnemyState.TooClose:
                case PatrolEnemyState.PatrolStuck:
                case PatrolEnemyState.ChaseStuck:
                    break;
            }

            OnEnemyStateEnter.Invoke(newState);

            _currentState = newState;
        }
        #endregion

        #region External AI Management
        public void PauseAI()
        {
            _rb.linearVelocityX = 0;
            _isAiPaused = true;
            OnEnemyStateExit.Invoke(_currentState);
        }

        public void ResumeAI()
        {
            OnEnemyStateEnter.Invoke(_currentState);
            _isAiPaused = false;
        }
        #endregion

        #region Direction Manipulators

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
        #endregion

        #region Helper Functions
        private float GetVelocityModifiers()
        {
            // Account for direction velocity should be going as well as an adjustment in magnitude from time power.
            return _directionModifier * TimeManager.CurrentTimeModifier;
        }
        private bool IsHittingWall()
        {
            Vector2 wallCheckPosition = GetWallCheckPosition();
            bool isHittingWall = Physics2D.OverlapBox(wallCheckPosition, wallCheckSize, 0f, groundLayer);
            return isHittingWall;
        }

        private bool IsNearLedge()
        {
            // If enemy is moving on the y axis, return false.
            if (_rb.linearVelocityY != 0) return false;
            Vector2 ledgeCheckPosition = GetLedgeCheckPosition();
            bool isHittingFloor = Physics2D.OverlapBox(ledgeCheckPosition, ledgeCheckSize, 0f, groundLayer);
            return !isHittingFloor;
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

        private float GetSignedHorizontalDistanceTo(Vector3 other) => transform.position.x - other.x;

        private float GetEuclideanDistanceTo(Vector2 other) => Vector2.Distance(transform.position, other);

        private Vector2 GetWallCheckPosition() => transform.position + new Vector3(wallCheckOffset.x * _directionModifier, wallCheckOffset.y, wallCheckOffset.z);

        private Vector2 GetLedgeCheckPosition() => transform.position + new Vector3(ledgeCheckOffset.x * _directionModifier, ledgeCheckOffset.y, ledgeCheckOffset.z);
        #endregion

        private void OnDrawGizmosSelected()
        {
            Vector2 wallCheckPosition = GetWallCheckPosition();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(wallCheckPosition, wallCheckSize);

            Vector2 ledgeCheckPosition = GetLedgeCheckPosition();
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(ledgeCheckPosition, ledgeCheckSize);
        }
    }
}

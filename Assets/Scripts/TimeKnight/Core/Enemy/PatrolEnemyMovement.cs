using System;
using TimeKnight.Core.Player;
using UnityEngine;

namespace TimeKnight.Core.Enemy
{
    public enum EnemyPatrolState
    {
        Patrol,
        Chase,
        LostSight,
        TooClose,
    }
    
    public class PatrolEnemyMovement : MonoBehaviour
    {
        private Rigidbody2D _rb = null!;

        public EnemyPatrolState CurrentState { get; private set; } = EnemyPatrolState.Patrol;
        private Vector3 _playerPosition => PlayerController.PlayerPosition;

        // AI Management from enemy script. Actions allow enemy script to react when enemy state changes.
        private bool _isAiPaused = false;
        public event Action<EnemyPatrolState> OnEnemyStateExit = delegate { };
        public event Action<EnemyPatrolState> OnEnemyStateEnter = delegate { };

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
            OnEnemyStateEnter.Invoke(CurrentState);
        }

        # region AI Logic / State Management
        public void Update()
        {
            if (_isAiPaused) return;

            IsPlayerChaseable = CheckIsPlayerChaseable();

            switch (CurrentState)
            {
                case EnemyPatrolState.Patrol:
                    Patrol();
                    break;
                case EnemyPatrolState.Chase:
                    Chase();
                    break;
                case EnemyPatrolState.LostSight:
                    LostSight();
                    break;
                case EnemyPatrolState.TooClose:
                    TooClose();
                    break;
            }
        }

        private void Patrol()
        {
            if (IsPlayerChaseable)
            {
                TransitionTo(EnemyPatrolState.Chase);
                return;
            }

            bool _isMovingOutOfPatrolRange = IsMovingOutOfPatrolRange();
            bool _isOutOfPatrolRange = IsOutOfPatrolRange();
            bool _isHittingWall = IsHittingWall();
            bool _isNearLedge = IsNearLedge();

            // 1. Check if the enemy needs to be flipped. We need to flip if we are moving away from patrol zone, or if we are in patrol zone but hitting wall or ledge.
            if (_isMovingOutOfPatrolRange || !_isOutOfPatrolRange && (_isHittingWall || _isNearLedge))
            {
                FlipPatrolDirection();
            }

            // 2. Calculate if we are away from zone, and teleport back if timer reaches end.
            if (_isOutOfPatrolRange)
            {
                _timeAwayFromPatrol += Time.deltaTime;
            }
            else
            {
                _timeAwayFromPatrol = 0f;
            }

            if (_timeAwayFromPatrol >= _teleportReturnTime)
            {
                transform.position = _patrolAnchor;
            }

           // 3. If we are out of patrol range and hitting wall or near a ledge, stop moving - the return to teleport timer will bring enemy back.
           //    Otherwise move like normal.
            if (_isOutOfPatrolRange && (_isHittingWall || _isNearLedge))
            {
                _rb.linearVelocityX = 0;
            }
            else
            {
                _rb.linearVelocityX = patrolWalkSpeed * _directionModifier;
            }
        }

        private void Chase()
        {
            if (!IsPlayerChaseable)
            {
                TransitionTo(EnemyPatrolState.LostSight);
                return;
            }

            // If enemy is too close to the player, move into too close state
            if (GetAbsoluteHorizontalDistanceTo(_playerPosition) <= minimumPlayerDistance)
            {
                TransitionTo(EnemyPatrolState.TooClose);
                return;
            }

            bool isPlayerToRight = _playerPosition.x > transform.position.x;

            if (isPlayerToRight)
            {
                FaceRight();
            }
            else
            {
                FaceLeft();
            }

            _rb.linearVelocityX = chaseWalkSpeed * _directionModifier;
        }

        private void LostSight()
        {
            // Chase player if sight is returned, but otherwise wait for cooldown before returning to patrol.
            if (IsPlayerChaseable)
            {
                TransitionTo(EnemyPatrolState.Chase);
            }
            else if (_lostSightTimer >= lostSightPatrolCooldown)
            {
                TransitionTo(EnemyPatrolState.Patrol);
            }

            _rb.linearVelocityX = 0;

            _lostSightTimer += Time.deltaTime;
        }

        private void TooClose()
        {
            // Don't transition back to chasing until the player is a bit past minium distance.
            // This prevents freaking out when player is at edge of minimum distance 
            if (GetAbsoluteHorizontalDistanceTo(_playerPosition) > minimumPlayerDistance + 0.5)
            {
                TransitionTo(EnemyPatrolState.Chase);
            }
            _rb.linearVelocityX = 0;
        }

        private void TransitionTo(EnemyPatrolState newState)
        {
            if (CurrentState == newState) return;

            OnEnemyStateExit.Invoke(CurrentState);

            // Perform actions needed for entering new state, then enter new state;
            switch (newState)
            {
                case EnemyPatrolState.Patrol: _timeAwayFromPatrol = 0f; break;
                case EnemyPatrolState.LostSight: _lostSightTimer = 0f; break;
                case EnemyPatrolState.Chase:
                case EnemyPatrolState.TooClose:
                    break;
            }

            OnEnemyStateEnter.Invoke(newState);

            CurrentState = newState;
        }
        #endregion

        #region External AI Management
        public void PauseAI()
        {
            _rb.linearVelocityX = 0;
            _isAiPaused = true;
            OnEnemyStateExit.Invoke(CurrentState);
        }

        public void ResumeAI()
        {
            OnEnemyStateEnter.Invoke(CurrentState);
            _isAiPaused = false;
        }
        #endregion

        #region Direction Manipulators
        private void FlipPatrolDirection()
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

        private bool IsMovingOutOfPatrolRange()
        {
            // For basic patrol enemy, we only care about horizontal distance because these enemies cannot jump.
            float horizontalDistance = GetSignedHorizontalDistanceTo(_patrolAnchor);

            // If enemy moving right and has gone past patrol range in positive x, enemy out of range.
            if (!_isFacingLeft && horizontalDistance > patrolRange)
            {
                return true;
            }
            // If enemy moving left, and we went past range in negative x, enemy out of range.
            else if (_isFacingLeft && horizontalDistance < patrolRange * -1)
            {
                return true;
            }

            return false;
        }

        private bool IsOutOfPatrolRange()
        {
            return GetAbsoluteHorizontalDistanceTo(_patrolAnchor) > patrolRange;
        }

        private bool CheckIsPlayerChaseable()
        {
            // Only perform raycast if player is within chaseable range.
            if (GetAbsoluteHorizontalDistanceTo(_playerPosition) > chaseRange) return false;

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

        private float GetSignedHorizontalDistanceTo(Vector3 other)
        {
            return transform.position.x - other.x;
        }

        private float GetAbsoluteHorizontalDistanceTo(Vector3 other)
        {
            return Math.Abs(GetSignedHorizontalDistanceTo(other));
        }

        private Vector2 GetWallCheckPosition()
        {
            return transform.position + new Vector3(wallCheckOffset.x * _directionModifier, wallCheckOffset.y, wallCheckOffset.z);
        }

        private Vector2 GetLedgeCheckPosition()
        {
            return transform.position + new Vector3(ledgeCheckOffset.x * _directionModifier, ledgeCheckOffset.y, ledgeCheckOffset.z);
        }

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

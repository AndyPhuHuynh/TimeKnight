using System;
using TimeKnight.Core.Player;
using UnityEngine;

namespace TimeKnight.Core.Enemy
{
    public class BasicEnemy : MonoBehaviour
    {
        private Rigidbody2D _rb = null!;

        private EnemyState _currentState = EnemyState.Patrol;
        private Vector3 _playerPosition => PlayerController.PlayerPosition;

        [Header("Patrol AI")]
        [SerializeField] private float patrolWalkSpeed = 3f;
        [SerializeField] private float patrolRange = 10f;
        private Vector3 _patrolAnchor;

        [Header("Chase AI")]
        [SerializeField] private float chaseRange = 5f;
        [SerializeField] private float chaseWalkSpeed = 4f;
        [SerializeField] private float minimumPlayerDistance = 0.2f;

        [Header("Collision")]
        [SerializeField] private Vector3 wallCheckOffset = new Vector3(1f, 0f, 0f);
        [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f, 1f);
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask playerLayer;

        // Directional variables
        private bool _isFacingLeft => transform.localScale.x < 0;
        private int _directionModifier => _isFacingLeft ? -1 : 1;
        private float _baseScaleX;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _patrolAnchor = transform.position;
            _baseScaleX = Math.Abs(transform.localScale.x);
        }

        public void Update()
        {
            if (IsPlayerChaseable())
            {
                _currentState = EnemyState.Chase;
            }
            else
            {
                _currentState = EnemyState.Patrol;
            }

            switch (_currentState)
            {
                case EnemyState.Patrol:
                    Patrol();
                    break;
                case EnemyState.Chase:
                    Chase();
                    break;
            }
        }

        private void Patrol()
        {
            if (IsHittingWall() || IsOutOfPatrolRange())
            {
                FlipPatrolDirection();
            }

            _rb.linearVelocityX = patrolWalkSpeed * _directionModifier;
        }

        private void Chase()
        {
            // If enemy is too close to the player, return;
            float absolutePlayerDistance = Math.Abs(GetHorizontalDistanceTo(_playerPosition));
            if (absolutePlayerDistance <= minimumPlayerDistance)
            {
                _rb.linearVelocityX = 0;
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
            scale.x = _baseScaleX;
            transform.localScale = scale;
        }

        private void FaceLeft()
        {
            Vector3 scale = transform.localScale;
            scale.x = -_baseScaleX;
            transform.localScale = scale;
        }
        #endregion

        #region Helper Functions
        private bool IsHittingWall()
        {
            Vector2 wallCheckPosition = transform.position + (wallCheckOffset * _directionModifier);
            bool isHittingWall = Physics2D.OverlapBox(wallCheckPosition, wallCheckSize, 0f, groundLayer);
            return isHittingWall;
        }

        private bool IsOutOfPatrolRange()
        {
            // For basic patrol enemy, we only care about horizontal distance because these enemies cannot jump.
            float horizontalDistance = GetHorizontalDistanceTo(_patrolAnchor);

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

        private bool IsPlayerChaseable()
        {
            // Only perform raycast if player is within chaseable range.
            if (GetHorizontalDistanceTo(_playerPosition) > chaseRange) return false;

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

        private float GetHorizontalDistanceTo(Vector3 other)
        {
            // This is used over Vector3.distance because we need to get the sign to detect what direction we are past the other point.
            return transform.position.x - other.x;
        }
        #endregion

        private void OnDrawGizmosSelected()
        {
            Vector2 wallCheckPosition = transform.position + (wallCheckOffset * _directionModifier);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(wallCheckPosition, wallCheckSize);
        }
    }
}

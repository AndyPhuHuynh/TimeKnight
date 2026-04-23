namespace TimeKnight.Core.Enemy
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        LostSight,
    }

    public static class EnemyStateExtensions
    {
        public static bool IsPatrolling(this EnemyState state) => state == EnemyState.Patrol;
        public static bool IsChasing(this EnemyState state) => state == EnemyState.Chase;
        public static bool IsLostSight(this EnemyState state) => state == EnemyState.LostSight;
    }
}
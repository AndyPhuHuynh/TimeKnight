namespace TimeKnight.Core.Enemy
{
    public enum EnemyCombatState
    {
        None,
        Attacking,
        BeingDamaged
    }

    
    public static class HookStateExtensions
    {
        public static bool IsNone       (this EnemyCombatState state) => state == EnemyCombatState.None;
        public static bool IsAttacking  (this EnemyCombatState state) => state == EnemyCombatState.Attacking;
        public static bool IsBeingDamaged (this EnemyCombatState state) => state == EnemyCombatState.BeingDamaged;
    }
}
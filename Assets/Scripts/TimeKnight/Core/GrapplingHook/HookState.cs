namespace TimeKnight.Core.GrapplingHook
{
    public enum HookState
    {
        Idle = 0,
        Extending,
        Retracting,
        Stuck
    }

    public static class HookStateExtensions
    {
        public static bool IsIdle       (this HookState state) => state == HookState.Idle;
        public static bool IsExtending  (this HookState state) => state == HookState.Extending;
        public static bool IsRetracting (this HookState state) => state == HookState.Retracting;
        public static bool IsStuck      (this HookState state) => state == HookState.Stuck;   
    }
}
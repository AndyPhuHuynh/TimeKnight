using UnityEngine;

namespace TimeKnight.Core.GrapplingHook
{
    public class GrapplingHookTip : MonoBehaviour
    {
        [SerializeField] private GrapplingHook parentHook;
        public bool IsTipTouchingGround { get; private set; } = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsCollisionLayerGround(collision))
            {
                IsTipTouchingGround = true;
                if (parentHook.CurrentState.IsExtending())
                {
                    parentHook.TransitionTo(HookState.Stuck);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (IsCollisionLayerGround(collision))
            {
                IsTipTouchingGround = false;
            }
        }

        private static bool IsCollisionLayerGround(Collider2D collision)
        {
            return LayerMask.LayerToName(collision.gameObject.layer) == "Ground";
        }
    }
}
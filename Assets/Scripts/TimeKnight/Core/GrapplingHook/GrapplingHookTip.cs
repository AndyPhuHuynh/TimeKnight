using UnityEngine;

namespace TimeKnight.Core.GrapplingHook
{
    public class GrapplingHookTip : MonoBehaviour
    {
        [SerializeField] private GrapplingHook parentHook;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsCollisionLayerGround(collision) && parentHook.CurrentState.IsExtending())
            {
                parentHook.TransitionTo(HookState.Stuck);
            }
        }

        private static bool IsCollisionLayerGround(Collider2D collision)
        {
            return LayerMask.LayerToName(collision.gameObject.layer) == "Ground";
        }
    }
}
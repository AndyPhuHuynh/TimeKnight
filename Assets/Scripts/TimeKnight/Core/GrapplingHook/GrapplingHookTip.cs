using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core.GrapplingHook
{
    public class GrapplingHookTip : MonoBehaviour
    {
        [SerializeField] private GrapplingHook parentHook = null!;
        public bool IsTipTouchingGround { get; private set; }

        private void OnValidate()
        {
            Validation.NotNull(this, parentHook, nameof(parentHook));
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsCollisionLayerGrappleSurface(collision)) return;
            
            IsTipTouchingGround = true;
            if (parentHook.CurrentState.IsExtending())
            {
                parentHook.TransitionTo(HookState.Stuck);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (IsCollisionLayerGrappleSurface(collision))
            {
                IsTipTouchingGround = false;
            }
        }

        private bool IsCollisionLayerGrappleSurface(Collider2D collision)
        {
            // Yummy black magic ChatGPT line??? Yippee!!! God bless AI and burning forests
            // This allows us to change the grapple layer in inspector of grappling hook later on and not have it hard coded here.
            return (parentHook.GrappleSurfaceLayer.value & (1 << collision.gameObject.layer)) != 0;
        }
    }
}
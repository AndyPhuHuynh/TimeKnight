using UnityEngine;

namespace TimeKnight.Core.GrapplingHook
{
    public class GrapplingHookTip : MonoBehaviour
    {
        private ContactFilter2D _filter;
        private Collider2D _collider;
        private readonly Collider2D[] _collisionResults = new Collider2D[1];
        
        [SerializeField] private GrapplingHook parentHook;
        
        public bool IsTipTouchingGround { get; private set; }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                useTriggers = true
            };
            _filter.SetLayerMask(LayerMask.GetMask("Ground"));
        }

        private void FixedUpdate()
        {
            var count = _collider.Overlap(_filter, _collisionResults);
            IsTipTouchingGround = count > 0;
            if (parentHook.CurrentState.IsExtending() && IsTipTouchingGround)
            {
                parentHook.TransitionTo(HookState.Stuck);
            }
            
        }
    }
}
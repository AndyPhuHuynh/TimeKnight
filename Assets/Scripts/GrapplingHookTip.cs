using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHookTip : MonoBehaviour
{
    [SerializeField] private GrapplingHook _parentHook;
    // Reference to fire hook action needed in order to disable it when player is too close to a wall.
    private InputAction _fireHook;

    void Start()
    {
        _fireHook = InputSystem.actions.FindAction("Primary Fire");
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (IsCollisionLayerGround(collision))
        {
            _fireHook.Disable();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (IsCollisionLayerGround(collision))
        {
            _fireHook.Enable();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsCollisionLayerGround(collision))
        {
            _parentHook.HookTrigger();
        }
    }

    private bool IsCollisionLayerGround(Collider2D collision)
    {
        return LayerMask.LayerToName(collision.gameObject.layer) == "Ground";
    }
}

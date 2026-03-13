using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHookTip : MonoBehaviour
{
    [SerializeField] private GrapplingHook _parentHook;
    private InputAction _fireHook;
    private LayerMask _groundLayer;

    void Start()
    {
        _fireHook = InputSystem.actions.FindAction("Primary Fire");
        _groundLayer = LayerMask.GetMask("Ground");
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (_groundLayer == collision.gameObject.layer)
        {
            _fireHook.Disable();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (_groundLayer != collision.gameObject.layer)
        {
            _fireHook.Enable();

        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        _parentHook.HookTrigger();
    }
}

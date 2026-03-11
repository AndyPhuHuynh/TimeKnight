using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private readonly List<IInteractable> _interactables = new();
    private InputAction _interactAction;

    private void Awake()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void Update()
    {
        if (_interactAction.WasPressedThisFrame())
        {
            _interactables.ForEach(interactable => interactable.Interact());
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable == null) return;
        
        _interactables.Add(interactable);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable == null) return;
        
        _interactables.Remove(interactable);
    }
}

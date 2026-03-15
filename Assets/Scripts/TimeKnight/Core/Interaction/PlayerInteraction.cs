using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private GameObject interactionSelectorObject;

        private InputAction _interactAction;
        private SelectorUI _activeUI;
        
        private void Awake()
        {
            _interactAction = InputSystem.actions.FindAction("Interact");
            _activeUI = interactionSelectorObject.GetComponent<SelectorUI>();
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null) return;
            _activeUI.AddInteractable(interactable);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null) return;
            _activeUI.RemoveInteractable(interactable);
        }
    }
}

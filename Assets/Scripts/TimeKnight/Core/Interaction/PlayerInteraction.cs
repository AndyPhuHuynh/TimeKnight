using UnityEngine;

namespace TimeKnight.Core.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private SelectorUI interactionUI;
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null) return;
            interactionUI.AddInteractable(interactable);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null) return;
            interactionUI.RemoveInteractable(interactable);
        }
    }
}

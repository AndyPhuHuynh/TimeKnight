using UnityEngine;

namespace TimeKnight.Core.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null) return;
            InteractionEvents.RaiseInteractionTriggerEnter(interactable);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null) return;
            InteractionEvents.RaiseInteractionTriggerExit(interactable);
        }
    }
}

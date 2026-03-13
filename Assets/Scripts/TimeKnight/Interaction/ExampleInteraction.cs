using UnityEngine;

namespace TimeKnight.Interaction
{
    public class ExampleInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName;
        public string InteractionName => displayName ?? "ExampleInteraction";
        public void Interact()
        {
            Debug.Log($"Interacting with example object: {InteractionName}");
        }
    }
}

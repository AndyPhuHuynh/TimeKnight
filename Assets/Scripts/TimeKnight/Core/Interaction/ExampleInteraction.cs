using UnityEngine;

namespace TimeKnight.Core.Interaction
{
    public class ExampleInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Example Interaction";
        public string InteractionName => displayName;
        
        public void Interact()
        {
            Destroy(gameObject);
        }
    }
}

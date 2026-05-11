using TimeKnight.Core.Dialogue;
using UnityEngine;

namespace TimeKnight.Core.Interaction
{
    public class ExampleSlimeDialogue : MonoBehaviour, IInteractable
    {
        public string InteractionName => "Slime";

        public void Interact()
        {
            DialogueManager.Instance!.PlayDialogue("Start");
        }
    }
}

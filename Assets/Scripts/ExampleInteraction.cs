using UnityEngine;

public class ExampleInteraction : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Interacting with example object");
    }
}

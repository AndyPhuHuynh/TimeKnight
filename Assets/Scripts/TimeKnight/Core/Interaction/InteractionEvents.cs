using System;

namespace TimeKnight.Core.Interaction
{
    public static class InteractionEvents
    {
        public static Action<IInteractable> OnInteractionTriggerEnter;
        public static Action<IInteractable> OnInteractionTriggerExit;
        
        public static void RaiseInteractionTriggerEnter(IInteractable i) => OnInteractionTriggerEnter?.Invoke(i);
        public static void RaiseInteractionTriggerExit(IInteractable i) => OnInteractionTriggerExit?.Invoke(i);
    }
}
using System;

namespace TimeKnight.Core.Interaction
{
    public static class InteractionEvents
    {
        public static event Action<IInteractable> OnInteractionTriggerEnter = delegate {};
        public static event Action<IInteractable> OnInteractionTriggerExit  = delegate {};
        
        public static void RaiseInteractionTriggerEnter(IInteractable i) => OnInteractionTriggerEnter.Invoke(i);
        public static void RaiseInteractionTriggerExit(IInteractable i) => OnInteractionTriggerExit.Invoke(i);
    }
}
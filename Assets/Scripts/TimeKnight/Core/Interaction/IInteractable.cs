namespace TimeKnight.Core.Interaction
{
    public interface IInteractable
    {
        public string InteractionName { get; }
        public void Interact();
    }
}

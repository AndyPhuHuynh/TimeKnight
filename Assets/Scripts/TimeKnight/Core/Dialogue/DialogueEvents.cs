using System;

namespace TimeKnight.Core.Dialogue
{
    public static class DialogueEvents
    {
        public static event Action OnDialogueStart;
        public static event Action OnDialogueComplete;
        
        public static void RaiseStart() => OnDialogueStart?.Invoke();
        public static void RaiseComplete() => OnDialogueComplete?.Invoke();
    }
}
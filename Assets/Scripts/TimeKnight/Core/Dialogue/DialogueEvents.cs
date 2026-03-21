using System;

namespace TimeKnight.Core.Dialogue
{
    public static class DialogueEvents
    {
        public static Action OnDialogueStart;
        public static Action OnDialogueComplete;
        
        public static void RaiseStart() => OnDialogueStart?.Invoke();
        public static void RaiseComplete() => OnDialogueComplete?.Invoke();
    }
}
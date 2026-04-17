using System;

namespace TimeKnight.Core.Dialogue
{
    public static class DialogueEvents
    {
        public static event Action OnDialogueStart = delegate { };
        public static event Action OnDialogueComplete = delegate { };
        
        public static void RaiseStart() => OnDialogueStart.Invoke();
        public static void RaiseComplete() => OnDialogueComplete.Invoke();
    }
}
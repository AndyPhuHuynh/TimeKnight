using TimeKnight.Core.Input;
using TimeKnight.Utils;
using UnityEngine;
using Yarn.Unity;

namespace TimeKnight.Core.Dialogue
{
    [RequireComponent(typeof(DialogueRunner))]
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; } = null!;

        [Header("Input")]
        [SerializeField] private InputReader input = null!;
        
        private DialogueRunner _dialogueRunner = null!;
        private InputState? _previousInputState;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            Instance = this;
            
            _dialogueRunner = GetComponent<DialogueRunner>();
        }

        private void Start()
        {
            _dialogueRunner.onDialogueStart!.AddListener(OnDialogueStartFunc);
            _dialogueRunner.onDialogueComplete!.AddListener(OnDialogueCompleteFunc);
        }

        private void OnValidate()
        {
            Validation.NotNull(this, input, nameof(input));
        }

        private void OnDialogueStartFunc()
        {
            _previousInputState = input.SaveState();
            input.SetMapStatus(InputStatus.Disabled, ActionMaps.Every);
            input.SetMapStatus(InputStatus.Enabled, ActionMaps.Dialogue);
            
            DialogueEvents.RaiseStart();
        }

        private void OnDialogueCompleteFunc()
        {
            DialogueEvents.RaiseComplete();
            
            input.RestoreState(_previousInputState ?? default);
        }

        public void PlayDialogue(string dialogue)
        {
            if (!_dialogueRunner.Dialogue.NodeExists(dialogue))
            {
                Debug.LogError($"Dialogue \"{dialogue}\" does not exist");
                return;
            }
            _dialogueRunner.StartDialogue(dialogue);
        }
    }
}
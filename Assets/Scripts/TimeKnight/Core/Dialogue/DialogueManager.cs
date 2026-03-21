using System.Collections.Generic;
using TimeKnight.Core.Input;
using UnityEngine;
using Yarn.Unity;

namespace TimeKnight.Core.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("Input")]
        [SerializeField] private InputReader input;
        
        private DialogueRunner _dialogueRunner;
        private List<PreviousMapState> _previousInputMapStates;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            Instance = this;
            
            _dialogueRunner = GetComponent<DialogueRunner>();
            if (_dialogueRunner == null) Debug.LogError("Dialogue runner is null!");
        }

        private void Start()
        {
            _dialogueRunner.onDialogueStart!.AddListener(OnDialogueStartFunc);
            _dialogueRunner.onDialogueComplete!.AddListener(OnDialogueCompleteFunc);
        }

        private void OnValidate()
        {
            Debug.Assert(input          != null, $"Missing {nameof(input)}",          this);
        }

        private void OnDialogueStartFunc()
        {
            _previousInputMapStates = input.GetMapStates();
            input.EnableOnly(input.Actions.Dialogue);
            
            DialogueEvents.RaiseStart();
        }

        private void OnDialogueCompleteFunc()
        {
            DialogueEvents.RaiseComplete();
            
            InputReader.RestoreMapStates(_previousInputMapStates);
            _previousInputMapStates = null;
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
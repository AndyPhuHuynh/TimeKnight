using System;
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
        
        [Header("Dialogue")]
        [SerializeField] private DialogueRunner dialogueRunner;
        
        public Action OnDialogueStart;
        public Action OnDialogueComplete;

        private List<PreviousMapState> _previousInputMapStates;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            Instance = this;
        }

        private void Start()
        {
            dialogueRunner.onDialogueStart!.AddListener(OnDialogueStartFunc);
            dialogueRunner.onDialogueComplete!.AddListener(OnDialogueCompleteFunc);
        }

        private void OnValidate()
        {
            Debug.Assert(input          != null, $"Missing {nameof(input)}",          this);
            Debug.Assert(dialogueRunner != null, $"Missing {nameof(dialogueRunner)}", this);
        }

        private void OnDialogueStartFunc()
        {
            _previousInputMapStates = input.GetMapStates();
            input.EnableOnly(input.Actions.Dialogue);
            
            OnDialogueStart?.Invoke();
        }

        private void OnDialogueCompleteFunc()
        {
            OnDialogueComplete?.Invoke();
            
            InputReader.RestoreMapStates(_previousInputMapStates);
            _previousInputMapStates = null;
        }

        public void PlayDialogue(string dialogue)
        {
            if (!dialogueRunner.Dialogue.NodeExists(dialogue))
            {
                Debug.LogError($"Dialogue \"{dialogue}\" does not exist");
                return;
            }
            dialogueRunner.StartDialogue(dialogue);
        }
    }
}
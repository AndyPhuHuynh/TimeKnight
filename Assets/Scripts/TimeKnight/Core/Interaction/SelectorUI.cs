using System.Collections.Generic;
using TimeKnight.Core.Dialogue;
using TimeKnight.Core.Input;
using TimeKnight.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TimeKnight.Core.Interaction
{
    public class SelectorUI : MonoBehaviour
    {
        [Header("Input")] 
        [SerializeField] private InputReader input;
        
        [Header("Selector Button")]
        [SerializeField] private Button buttonPrefab;
        [SerializeField] private GameObject buttonContainer;
        [SerializeField] private GameObject buttonPoolContainer;
        
        [Header("Selector Cursor")]
        [SerializeField] private GameObject cursor;

        private CanvasGroup _canvasGroup;
        
        private SelectorButtonPool _buttonPool;
        private readonly Dictionary<IInteractable, SelectorButton> _buttonMap = new();
        
        private readonly LinkedList<SelectorButton> _activeButtons = new();
        private LinkedListNode<SelectorButton> _selectedButton;

        private bool _interactionAllowed = true;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _buttonPool = new SelectorButtonPool(buttonPrefab, buttonContainer, buttonPoolContainer);
            
            _canvasGroup.SetVisible(false);

            input.Actions.Interaction.Interact.performed += OnInteract;
            input.Actions.Interaction.Navigate.performed += OnNavigate;
        }
        
        private void OnValidate()
        {
            Debug.Assert(input               != null, $"Missing {nameof(input)}",               this);
            Debug.Assert(buttonPrefab        != null, $"Missing {nameof(buttonPrefab)}",        this);
            Debug.Assert(buttonContainer     != null, $"Missing {nameof(buttonContainer)}",     this);
            Debug.Assert(buttonPoolContainer != null, $"Missing {nameof(buttonPoolContainer)}", this);
            Debug.Assert(cursor              != null, $"Missing {nameof(cursor)}",              this);
        }

        private void OnEnable()
        {
            DialogueManager.Instance.OnDialogueStart += DisableInteractions;
            DialogueManager.Instance.OnDialogueComplete += EnableInteractions;
        }

        private void OnDisable()
        {
            DialogueManager.Instance.OnDialogueStart -= DisableInteractions;
            DialogueManager.Instance.OnDialogueComplete -= EnableInteractions;
        }

        private void EnableInteractions()
        {
            _interactionAllowed = true;
            _canvasGroup.SetVisible(!_activeButtons.IsEmpty());
        }

        private void DisableInteractions()
        {
            _interactionAllowed = false;
            _canvasGroup.SetVisible(false);
        }

        private void SetSelectedButton(LinkedListNode<SelectorButton> button)
        {
            _selectedButton = button;
            UpdateCursorPosition();
        }

        private void UpdateCursorPosition()
        {
            if (_selectedButton == null) return;
            
            var buttonRect = _selectedButton.Value.Button.transform as RectTransform;
            var rightEdgeLocal = new Vector3(buttonRect!.rect.xMax, buttonRect.rect.center.y, 0);
            var rightEdgeWorld = buttonRect!.transform.TransformPoint(rightEdgeLocal);
            
            cursor.transform.position = rightEdgeWorld + new Vector3(20.0f, 0.0f, 0.0f);
        }

        private void OnInteract(InputAction.CallbackContext _)
        {
            if (!_interactionAllowed) return;
            _selectedButton?.Value.Button.onClick?.Invoke();
        }

        private void OnNavigate(InputAction.CallbackContext ctx)
        {
            if (!_interactionAllowed) return;
            if (ctx.ReadValue<float>() > 0)
            {
                if (_selectedButton?.Previous == null) return;
                SetSelectedButton(_selectedButton.Previous);
            }
            else
            {
                if (_selectedButton?.Next == null) return;
                SetSelectedButton(_selectedButton.Next);
            }
        }
        
        public void AddInteractable(IInteractable interactable)
        {
            var button = _buttonPool.Pool.Get();
            button.Text.text = interactable.InteractionName;
            button.Button.onClick.AddListener(interactable.Interact);
            button.CanvasGroup.SetVisible(true);
            
            _buttonMap[interactable] = button;
            _activeButtons.AddLast(button);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonContainer.transform as RectTransform);
            UpdateCursorPosition();

            if (_activeButtons.Count > 1) return;
            SetSelectedButton(_activeButtons.First);
            if (_interactionAllowed)
            {
                _canvasGroup.SetVisible(true);
            }
        }

        public void RemoveInteractable(IInteractable interactable)
        {
            if (!_buttonMap.Remove(interactable, out var button)) return;
            if (_selectedButton != null && _selectedButton.Value == button)
            {
                _selectedButton = _selectedButton.Next ?? _selectedButton.Previous;
            }
            _buttonPool.Pool.Release(button);
            _activeButtons.Remove(button);
            
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            UpdateCursorPosition();
            
            if (!_buttonMap.IsEmpty()) return;
            _selectedButton = null;
            _canvasGroup.SetVisible(false);
        }
    }
}
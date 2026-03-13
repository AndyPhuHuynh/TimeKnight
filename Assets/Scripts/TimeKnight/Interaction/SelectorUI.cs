using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TimeKnight.Interaction
{
    public class SelectorUI : MonoBehaviour
    {
        [SerializeField] private Button selectorButtonPrefab;
        [SerializeField] private GameObject selectorButtonContainer;
        [SerializeField] private GameObject selectorButtonPoolContainer;
        [SerializeField] private GameObject selectorCursor;

        private SelectorButtonPool _buttonPool;
        private readonly Dictionary<IInteractable, SelectorButton> _buttonMap = new();
        private readonly List<SelectorButton> _activeButtons = new();
        private int _activeButtonIndex = -1;

        private void Awake()
        {
            _buttonPool = new SelectorButtonPool(selectorButtonPrefab, selectorButtonContainer, selectorButtonPoolContainer);
        }

        private void OnValidate()
        {
            Debug.Assert(selectorButtonPrefab        != null, $"Missing {nameof(selectorButtonPrefab)}",    this);
            Debug.Assert(selectorButtonContainer     != null, $"Missing {nameof(selectorButtonContainer)}", this);
            Debug.Assert(selectorButtonPoolContainer != null, $"Missing {nameof(selectorButtonContainer)}", this);
        }
        
        public void AddInteractable(IInteractable interactable)
        {
            var button = _buttonPool.Pool.Get();
            button.Text.text = interactable.InteractionName;
            button.Button.onClick.AddListener(interactable.Interact);
            button.Show();
            
            _buttonMap[interactable] = button;
            _activeButtons.Add(button);

            if (_activeButtons.Count <= 1)
            {
                
                gameObject.SetActive(true);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void RemoveInteractable(IInteractable interactable)
        {
            if (!_buttonMap.Remove(interactable, out var button)) return;
            _buttonPool.Pool.Release(button);
            _activeButtons.Remove(button);
            
            if (_buttonMap.Count == 0)
            {
                gameObject.SetActive(false);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }
}
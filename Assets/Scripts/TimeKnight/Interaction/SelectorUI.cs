using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeKnight.Interaction
{
    public readonly struct SelectorButton
    {
        public readonly Button Button;
        public readonly TextMeshProUGUI Text;
        private readonly CanvasGroup _canvasGroup;

        public SelectorButton(Button button)
        {
            Button = button;
            _canvasGroup = button.GetComponent<CanvasGroup>();
            Text = button.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        public void Hide()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void Show()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }
    
    public class SelectorUI : MonoBehaviour
    {
        [SerializeField] private Button selectorButtonPrefab;
        [SerializeField] private GameObject selectorButtonContainer;
        [SerializeField] private GameObject selectorButtonPoolContainer;
        
        private readonly Queue<SelectorButton> _buttonPool = new();
        private readonly Dictionary<IInteractable, SelectorButton> _buttonMap = new();

        private void Awake()
        {
            for (int i = 0; i < 5; i++)
            {
                var button = Instantiate(selectorButtonPrefab, selectorButtonContainer.transform);
                ReturnButton(new SelectorButton(button));
            }
        }

        private void OnValidate()
        {
            Debug.Assert(selectorButtonPrefab        != null, $"Missing {nameof(selectorButtonPrefab)}",    this);
            Debug.Assert(selectorButtonContainer     != null, $"Missing {nameof(selectorButtonContainer)}", this);
            Debug.Assert(selectorButtonPoolContainer != null, $"Missing {nameof(selectorButtonContainer)}", this);
        }
        
        private SelectorButton GetButton()
        {
            return _buttonPool.Count > 0 ? _buttonPool.Dequeue() : new SelectorButton(Instantiate(selectorButtonPrefab));
        }

        private void ReturnButton(SelectorButton button)
        {
            button.Hide();
            button.Button.transform.SetParent(selectorButtonPoolContainer.transform);
            button.Button.transform.SetAsFirstSibling();
            button.Button.onClick.RemoveAllListeners();
            _buttonPool.Enqueue(button);
        }
        
        public void AddInteractable(IInteractable interactable)
        {
            var button = GetButton();
            button.Button.transform.SetParent(selectorButtonContainer.transform);
            button.Button.transform.SetAsLastSibling();
            button.Text.text = interactable.InteractionName;
            button.Button.onClick.AddListener(interactable.Interact);
            button.Show();
            _buttonMap[interactable] = button;
            gameObject.SetActive(true);
        }

        public void RemoveInteractable(IInteractable interactable)
        {
            if (!_buttonMap.Remove(interactable, out var button)) return;
            ReturnButton(button);
            if (_buttonMap.Count == 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
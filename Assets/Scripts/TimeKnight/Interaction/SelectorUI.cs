using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeKnight.Interaction
{
    public class SelectorUI : MonoBehaviour
    {
        [SerializeField] private Button selectorButtonPrefab;
        [SerializeField] private GameObject selectorButtonContainer;
        [SerializeField] private GameObject selectorButtonPoolContainer;
        
        private readonly Queue<Button> _buttonPool = new();
        private readonly Dictionary<IInteractable, Button> _buttonMap = new();

        private void Awake()
        {
            for (int i = 0; i < 5; i++)
            {
                var button = InstantiateButton();
                ReturnButton(button);
            }
        }

        private Button InstantiateButton() => Instantiate(selectorButtonPrefab);

        private static void HideButton(Button button)
        {
            var cg = button.GetComponentInChildren<CanvasGroup>();
            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        private static void ShowButton(Button button)
        {
            var cg = button.GetComponentInChildren<CanvasGroup>();
            cg.alpha = 1;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        
        private Button GetButton()
        {
            return _buttonPool.Count > 0 ? _buttonPool.Dequeue() : InstantiateButton();
        }

        private void ReturnButton(Button button)
        {
            HideButton(button);
            button.transform.SetParent(selectorButtonPoolContainer.transform);
            button.transform.SetAsFirstSibling();
            _buttonPool.Enqueue(button);
        }
        
        public void AddInteractable(IInteractable interactable)
        {
            var button = GetButton();
            button.transform.SetParent(selectorButtonContainer.transform);
            button.transform.SetAsLastSibling();
            button.GetComponentInChildren<TextMeshProUGUI>().text = interactable.InteractionName;
            ShowButton(button);
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
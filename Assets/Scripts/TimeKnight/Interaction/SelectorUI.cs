using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeKnight.Interaction
{
    public class SelectorUI : MonoBehaviour
    {
        [SerializeField] private Button selectorButtonPrefab;
        [SerializeField] private GameObject selectorButtonPanel;

        private readonly Dictionary<IInteractable, Button> _buttonMap = new();
        
        public void AddInteractable(IInteractable interactable)
        {
            var button = Instantiate(selectorButtonPrefab, selectorButtonPanel.transform);
            button.GetComponentInChildren<TextMeshProUGUI>().text = interactable.InteractionName;
            _buttonMap[interactable] = button;
            gameObject.SetActive(true);
        }

        public void RemoveInteractable(IInteractable interactable)
        {
            if (!_buttonMap.Remove(interactable, out var button)) return;
            Destroy(button.gameObject);
            if (_buttonMap.Count == 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
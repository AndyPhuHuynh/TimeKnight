using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeKnight.Interaction
{
    public class SelectorButton : IEquatable<SelectorButton>
    {
        public readonly Button Button;
        public readonly TextMeshProUGUI Text;
        private readonly CanvasGroup _canvasGroup;

        public SelectorButton(Button button)
        {
            Button = button;
            Text = button.GetComponentInChildren<TextMeshProUGUI>();
            _canvasGroup = button.GetComponent<CanvasGroup>();
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

        public bool Equals(SelectorButton other)
        {
            return other != null && ReferenceEquals(other.Button, Button);
        }
    }
}
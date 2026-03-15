using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeKnight.Core.Interaction
{
    public class SelectorButton : IEquatable<SelectorButton>
    {
        public readonly Button Button;
        public readonly CanvasGroup CanvasGroup;
        public readonly TextMeshProUGUI Text;

        public SelectorButton(Button button)
        {
            Button = button;
            CanvasGroup = button.GetComponent<CanvasGroup>();
            Text = button.GetComponentInChildren<TextMeshProUGUI>();
        }

        public bool Equals(SelectorButton other)
        {
            return other != null && ReferenceEquals(other.Button, Button);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace TimeKnight.Utils
{
    public static class InputUtils
    {
        private static List<ButtonControl> GetButtonsFromAction(InputAction action)
        {
            var buttons = new List<ButtonControl>();
            foreach (var control in action.controls)
            {
                if (control is ButtonControl button)
                    buttons.Add(button);
            }
            return buttons;
        }
        
        public static bool IsPressedRegardlessOfEnableStatus(this InputAction action)
        {
            var buttons = GetButtonsFromAction(action);
            return buttons.Any(control => control.isPressed);
        }
    }
}
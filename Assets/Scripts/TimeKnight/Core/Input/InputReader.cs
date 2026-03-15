using UnityEngine;

namespace TimeKnight.Core.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
    public class InputReader : ScriptableObject
    {
        public PlayerInputActions Actions { get; private set; }

        private void OnEnable()
        {
            Actions = new PlayerInputActions();
            Actions.Enable();
        }

        private void OnDisable()
        {
            Actions.Disable();
        }
    }
}

using TimeKnight.Core.Input;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeKnight.Core.Player
{
    public class PlayerSwordController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputReader input = null!;
        [Header("Sword")]
        [SerializeField] private Sword.Sword sword = null!;

        private void OnValidate()
        {
            Validation.NotNull(this, input, nameof(input));
            Validation.NotNull(this, sword, nameof(sword));
        }

        private void OnEnable()
        {
            input.Actions.Sword.Attack.performed += OnSwordAttackPreformed;
        }

        private void OnDisable()
        {
            input.Actions.Sword.Attack.performed -= OnSwordAttackPreformed;            
        }

        private void OnSwordAttackPreformed(InputAction.CallbackContext _)
        {
            sword.BeginSwing();
        }
    }
}
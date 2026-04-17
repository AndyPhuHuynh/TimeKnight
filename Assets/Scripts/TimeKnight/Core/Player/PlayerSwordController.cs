using TimeKnight.Core.Input;
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
            Debug.Assert(input != null, $"Input Reader on {gameObject.name} not defined");
            Debug.Assert(sword != null, $"Sword on {gameObject.name} not defined");
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
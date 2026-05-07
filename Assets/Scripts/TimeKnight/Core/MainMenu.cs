using TimeKnight.Core.Scene;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TimeKnight.Core
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button startButton = null!;
        [SerializeField] private SceneReference level = null!;

        private void OnValidate()
        {
            Validation.NotNull(this, startButton, nameof(startButton));
            Validation.NotNull(this, level, nameof(level));
        }

        private void Awake()
        {
            startButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(level.SceneName);
            });
        }
    }
}

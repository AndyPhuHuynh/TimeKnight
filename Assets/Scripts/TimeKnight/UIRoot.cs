using UnityEngine;

namespace TimeKnight
{
    public class UIRoot : MonoBehaviour
    {
        [SerializeField] private GameObject mainCanvas;
        public GameObject MainCanvas => mainCanvas;

        public static UIRoot Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }
}

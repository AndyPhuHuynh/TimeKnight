using UnityEngine;

namespace TimeKnight.Core
{
    public class CameraScript : MonoBehaviour
    {
        [SerializeField] private GameObject player = null!;

        private void OnValidate()
        {
            Debug.Assert(player != null, $"Missing {nameof(player)}", this);
        }

        private void Update()
        {
            transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        }
    }
}

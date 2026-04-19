using TimeKnight.Utils;
using UnityEngine;

namespace TimeKnight.Core
{
    public class CameraScript : MonoBehaviour
    {
        [SerializeField] private GameObject player = null!;

        private void OnValidate()
        {
            Validation.NotNull(this, player, nameof(player));
        }

        private void Update()
        {
            transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        }
    }
}

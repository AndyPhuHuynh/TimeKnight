using UnityEngine;

namespace TimeKnight.Core
{
    public class CameraScript : MonoBehaviour
    {
        [SerializeField] private GameObject player;

        private void Update()
        {
            transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        }
    }
}

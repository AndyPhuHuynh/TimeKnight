using UnityEngine;

namespace TimeKnight.Core
{
    public class CameraScript : MonoBehaviour
    {
        private GameObject? _player;
        
        private void Update()
        {
            if (_player == null) return;
            transform.position = new Vector3(_player.transform.position.x, _player.transform.position.y, transform.position.z);
        }

        public void Initialize(GameObject player)
        {
            _player = player;
        }
    }
}

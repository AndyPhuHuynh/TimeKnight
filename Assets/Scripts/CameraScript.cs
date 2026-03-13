using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(_player.transform.position.x, transform.position.y, transform.position.z);
    }
}

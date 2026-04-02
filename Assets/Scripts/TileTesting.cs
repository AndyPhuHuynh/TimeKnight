using UnityEngine;
using UnityEngine.Tilemaps;

public class TileTesting : MonoBehaviour
{
    private Tilemap _tilemap;
    
    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();
    }

    private void Start()
    {
        Debug.Log($"{_tilemap.cellBounds}: Start");
    }
}

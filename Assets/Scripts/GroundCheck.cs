using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool isGrounded {get; private set;} = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if ("Ground" == LayerMask.LayerToName(collision.gameObject.layer))
        {
            isGrounded = true;
        }
    }

  void OnTriggerExit2D(Collider2D collision)
  {
    if ("Ground" == LayerMask.LayerToName(collision.gameObject.layer))
        {
            isGrounded = false;
        }
  }
}

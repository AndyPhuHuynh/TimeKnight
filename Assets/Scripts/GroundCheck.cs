using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool IsGrounded { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ("Ground" == LayerMask.LayerToName(collision.gameObject.layer))
        {
            IsGrounded = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
      {
        if ("Ground" == LayerMask.LayerToName(collision.gameObject.layer))
        {
            IsGrounded = false;
        }
      }
}

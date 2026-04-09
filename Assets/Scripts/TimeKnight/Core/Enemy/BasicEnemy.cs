using TimeKnight.Core.Player;
using UnityEngine;

namespace TimeKnight.Core.Enemy
{
    public class BasicEnemy : MonoBehaviour
    {
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.tag != "PlayerManager") return;

            collision.gameObject.GetComponent<PlayerManager>().TakeDamage(3);
        }
    }
}
using UnityEngine;

namespace TimeKnight.Utils
{
    public static class Combat
    {
        public static Vector2 CalculateKnockback(Vector2 source, Vector2 target, float horizontalKnockbackForce, float verticalKnockbackForce)
        {
            var horizontalDirection = target.x >= source.x ? 1f : -1f;

            return new Vector2(horizontalDirection * horizontalKnockbackForce, verticalKnockbackForce);
        }

        public static void ApplyKnockback(Rigidbody2D rb, Vector2 knockback)
        {
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }
    }

}
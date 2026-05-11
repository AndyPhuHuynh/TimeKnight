using UnityEngine;

namespace TimeKnight.Core.Enemy
{
    public interface IDamageable
    {
        public void Damage(float damage, Vector2? knockback = null);
    }
}

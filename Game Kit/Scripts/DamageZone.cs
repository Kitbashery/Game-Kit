using UnityEngine;

namespace Kitbashery.core
{
    [AddComponentMenu("Kitbashery/Gameplay/Damage Zone")]
    public class DamageZone : CollisionEvents
    {
        [Header("Damage Zone:")]
        [Space]
        public int damage = 1;

        private Health targetHP;

        public void ApplyDamage()
        {
            if (targetHP != null)
            {
                targetHP.ApplyDamage(damage);
            }
        }

        public void RegisterTarget()
        {
            targetHP = lastContact.gameObject.GetComponent<Health>();
        }

        public void UnregisterTarget()
        {
            targetHP = null;
        }
    }
}
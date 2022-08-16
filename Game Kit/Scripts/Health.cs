using UnityEngine;
using UnityEngine.Events;

namespace Kitbashery.Gameplay
{
    /// <summary>
    /// Tracks "hit points" and invokes events based on the current amount or state.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/game-kit/health.html")]
    [AddComponentMenu("Kitbashery/Gameplay/Health")]
    public class Health : MonoBehaviour
    {
        #region Properties:

        [Tooltip("Check this to disable receiving damage.")]
        public bool invinsible = false;
        [Tooltip("If assigned the hitpoints of the bonus health will need to be depleted before damage can be applied to this health component.")]
        public Health bonusHealth;
        [Tooltip("The hitpoints to start with, it isn't recommended to modify this value directly during runtime, use ApplyDamage() instead.")]
        [Min(0)]
        public int hitPoints = 100;
        public bool canRegen = false;
        [Tooltip("Hitpoints to regenerate.")]
        [Min(1)]
        public int regenPoints = 1;
        [Tooltip("time between regenerating hitpoints (in seconds).")]
        [Min(0.01f)]
        public float regenRate = 0.1f;

        private int originalHitpoints = 1;
        private float halfHealth = 0.5f;
        private float currentRegenTime = 0;

        [Space]
        public UnityEvent onReceivedDamage;
        public UnityEvent onHealed;
        public UnityEvent onDead;

        #endregion

        #region Initialization & Updates:

        private void Awake()
        {
            originalHitpoints = hitPoints;
            halfHealth = originalHitpoints / 2;
        }

        private void Update()
        {
            if (canRegen == true && hitPoints < originalHitpoints)
            {
                if (currentRegenTime >= regenRate)
                {
                    hitPoints += regenPoints;
                    currentRegenTime = 0;
                }
                else
                {
                    currentRegenTime += Time.deltaTime;
                }
                onHealed.Invoke();
            }
        }

        #endregion

        #region Core Functions:

        /// <summary>
        /// Applies damage, if there is bonus health damage will be applied to that component first.
        /// </summary>
        /// <param name="amount">The amount of damage to apply.</param>
        public void ApplyDamage(int amount)
        {
            if (invinsible == false && hitPoints > 0)
            {
                if (bonusHealth != null)
                {
                    if (bonusHealth.hitPoints <= 0)
                    {
                        hitPoints -= amount;
                        onReceivedDamage.Invoke();
                    }
                    else
                    {
                        bonusHealth.ApplyDamage(amount);
                    }
                }
                else
                {
                    hitPoints -= amount;
                    onReceivedDamage.Invoke();
                }

                if (hitPoints <= 0)
                {
                    onDead.Invoke();
                }
            }
        }

        /// <summary>
        /// Instantly regenerates hitpoints if canRegen is true.
        /// </summary>
        public void ClearRegenCooldown()
        {
            currentRegenTime = 0;
        }

        /// <summary>
        /// Instantly heals the specified amount of hitpoints.
        /// </summary>
        /// <param name="amount">The amount of hitpoints to heal.</param>
        public void Heal(int amount)
        {
            if (hitPoints < originalHitpoints)
            {
                hitPoints += amount;
                onHealed.Invoke();
            }
        }

        /// <summary>
        /// Resets the hitpoints of all health and (optionally) all bonus health components in the chain to their original values. Useful for pooling.
        /// </summary>
        /// <param name="resetBonusHealth">If true will reset the bonus health recursively until no bonus health is found.</param>
        public void ResetHealth(bool resetBonusHealth = false)
        {
            hitPoints = originalHitpoints;
            if (bonusHealth != null && resetBonusHealth == true)
            {
                bonusHealth.ResetHealth(resetBonusHealth);
            }
        }

        /// <summary>
        /// Overrides the initial hitpoint value and adjusts the current hitpoints to fit within the new range.
        /// </summary>
        /// <param name="amount">The new max value of hitpoints.</param>
        public void SetMaxHitpoints(int amount)
        {
            if (amount != originalHitpoints && amount > 0)
            {
                if (amount > originalHitpoints)
                {
                    hitPoints += (originalHitpoints - amount);
                }
                else
                {
                    if (hitPoints > amount)
                    {
                        hitPoints = amount;
                    }
                }
                originalHitpoints = amount;
                halfHealth = originalHitpoints / 2;
            }
        }

        /// <summary>
        /// Checks if the amount of hitpoints is below 50% of the intial value.
        /// </summary>
        /// <returns>true if hitpoints are below 50%</returns>
        public bool IsLessThanHalfHealth()
        {
            return halfHealth < hitPoints;
        }

        public void DebugCurrentHealth()
        {
            Debug.Log("|Health|: on " + gameObject.name + " hitpoints currently are: " + hitPoints);
        }

        #endregion
    }
}
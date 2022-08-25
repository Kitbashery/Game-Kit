using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 MIT License

Copyright (c) 2022 Kitbashery

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.



Need support or additional features? Please visit https://kitbashery.com/
*/

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

        [field: SerializeField, Tooltip("Check this to disable receiving damage.")]
        public bool invinsible { get; set; } = false;

        [Tooltip("If assigned the hitpoints of the bonus health will need to be depleted before damage can be applied to this health component.")]
        public Health bonusHealth;
        [Tooltip("The hitpoints to start with, it isn't recommended to modify this value directly during runtime, use ModifyHealth() instead.")]
        [Min(0)]
        public int hitPoints = 100;
        [field: SerializeField, Tooltip("Should the maximum hit points be expanded if healed beyond maxHitPoints?")]
        public bool overhealExpand { get; set; } = false;

        [field: SerializeField, Tooltip("Enables passive health regeneration.")]
        public bool canRegen { get; set; } = false;
        [Tooltip("Hitpoints to regenerate.")]
        [Min(1)]
        public int regenPoints = 1;
        [Tooltip("Time between regenerating hitpoints (in seconds).")]
        [Min(0.01f)]
        public float regenRate = 0.1f;

        private int originalHitpoints = 100;
        private int maxHitpoints = 100;
        /// <summary>
        /// Half the current max value of <see cref="hitPoints"/>.
        /// </summary>
        private float halfHealth = 50f;
        private float currentRegenTime = 0;

        private Dictionary<TimedHealthEffect, Coroutine> healthEffects = new Dictionary<TimedHealthEffect, Coroutine>();

        [Space]
        public UnityEvent onReceivedDamage;
        public UnityEvent onHealed;
        public UnityEvent onDead;

        #endregion

        #region Initialization & Updates:

        private void Awake()
        {
            originalHitpoints = hitPoints;
            maxHitpoints = hitPoints;
            halfHealth = maxHitpoints / 2;

            if(bonusHealth == this)
            {
                bonusHealth = null;
                Debug.Log("|Health|: Has bonusHealth assigned to itself so it has been set to null.", gameObject);
            }
        }

        private void Update()
        {
            if (canRegen == true && hitPoints < maxHitpoints)
            {
                if (currentRegenTime >= regenRate)
                {
                    hitPoints = Mathf.Clamp(hitPoints + regenPoints, 0, maxHitpoints);
                    currentRegenTime = 0;
                }
                else
                {
                    currentRegenTime += Time.deltaTime;
                }
                onHealed.Invoke();
            }
        }
        private IEnumerator ExecuteTimedHealthEffect(TimedHealthEffect effect)
        {
            for (int i = 0; i < effect.times; i++)
            {
                ModifyHealth(effect.modifier, effect.amount);

                yield return new WaitForSeconds(effect.interval);
                if (i == effect.times)
                {
                    healthEffects.Remove(effect);
                }
            }
        }

        #endregion

        #region Core Functions:

        public void ModifyHealth(HealthModifiers modifier, int amount)
        {
            switch(modifier)
            {
                case HealthModifiers.damage:

                    ApplyDamage(amount);

                    break;

                case HealthModifiers.heal:

                    Heal(amount);

                    break;
            }
        }

        /// <summary>
        /// Starts a <see cref="TimedHealthEffect"/> that will modify health over time.
        /// </summary>
        /// <param name="effect"></param>
        public void ModifyHealthOverTime(TimedHealthEffect effect)
        {
            healthEffects.Add(effect, StartCoroutine(ExecuteTimedHealthEffect(effect)));
        }

        /// <summary>
        /// Stops all timed health effects that use a specific health modifier.
        /// </summary>
        /// <param name="modifier"></param>
        public void StopTimedEffectsWithModifier(HealthModifiers modifier)
        {
            for (int i = healthEffects.Count; i > 0; i--)
            {
                TimedHealthEffect effect = healthEffects.Keys.ElementAt(i);
                if(effect.modifier == modifier)
                {
                    StopCoroutine(healthEffects[effect]);
                    healthEffects.Remove(effect);
                }
            }
        }

        /// <summary>
        /// Stops a specific health effect by its <see cref="TimedHealthEffect"/> name.
        /// </summary>
        /// <param name="effectName"></param>
        public void StopTimedHealthEffect(string effectName)
        {
            for(int i = healthEffects.Count; i > 0; i--)
            {
                TimedHealthEffect effect = healthEffects.Keys.ElementAt(i);
                if (!string.IsNullOrEmpty(effectName))
                {
                    if (effect.name == effectName)
                    {
                        StopCoroutine(healthEffects[effect]);
                        healthEffects.Remove(effect);
                    }
                }
                else
                {
                    StopCoroutine(healthEffects[effect]);
                    healthEffects.Remove(effect);
                }
            }
        }

        public void StopAllTimedHealthEffects()
        {
            for (int i = healthEffects.Count; i > 0; i--)
            {
                TimedHealthEffect effect = healthEffects.Keys.ElementAt(i);
                StopCoroutine(healthEffects[effect]);
                healthEffects.Remove(effect);
            }
        }

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
            if (hitPoints < maxHitpoints)
            {
                hitPoints += amount;
                if (maxHitpoints - hitPoints < 0)
                {
                    if(overhealExpand == true)
                    {
                        SetMaxHitpoints(hitPoints);
                    }
                    hitPoints = maxHitpoints;
                }
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
            if (amount != maxHitpoints && amount > 0)
            {
                if (amount > maxHitpoints)
                {
                    hitPoints += (maxHitpoints - amount);
                }
                else
                {
                    if (hitPoints > amount)
                    {
                        hitPoints = amount;
                    }
                }
                maxHitpoints = amount;
                halfHealth = maxHitpoints / 2;
            }
        }

        /// <summary>
        /// Use to update healbar UI.
        /// </summary>
        /// <param name="healthbar">UI slider to update.</param>
        public void UpdateHealthbar(UnityEngine.UI.Slider healthbar)
        {
            if(healthbar.maxValue != maxHitpoints)
            {
                healthbar.maxValue = maxHitpoints;
            }
            healthbar.value = hitPoints;
        }

        /// <summary>
        /// Checks if the amount of hitpoints is below 50% of the intial value.
        /// </summary>
        /// <returns>true if hitpoints are below 50%</returns>
        public bool IsLessThanHalfHealth()
        {
            return halfHealth < hitPoints;
        }

        /// <summary>
        /// Checks if there are any heal/damage over time effects being apllied.
        /// </summary>
        /// <returns>true if there are effect playing.</returns>
        public bool TimedEffectsPlaying()
        {
            return healthEffects.Count > 0;
        }

        /// <summary>
        /// Checks if there is a specific timed health effect playing.
        /// </summary>
        /// <param name="effectName">The name of the effect to check for.</param>
        /// <returns>true if an effect is found.</returns>
        public bool HasTimedHealthEffect(string effectName)
        {
            foreach(TimedHealthEffect key in healthEffects.Keys)
            {
                if(key.name == effectName)
                {
                    return true;
                }
            }
            return false;
        }

        public void DebugTimedEffectCount()
        {
            Debug.LogFormat(gameObject, "|Health|: Has {0} timed health effects being applied.", healthEffects.Count);
        }

        public void DebugCurrentHealth()
        {
            Debug.LogFormat(gameObject, "|Health|: Hitpoints currently are: {0}", hitPoints);
        }

        #endregion
    }

    [Serializable]
    public struct TimedHealthEffect
    {
        [Tooltip("The name of the effect, used for stopping effects with this name.")]
        public string name;
        public HealthModifiers modifier;
        [Tooltip("The amount of healing or damage to apply.")]
        [Min(1)]
        public int amount;
        [Tooltip("The amount of times to modify health.")]
        [Min(1)]
        public int times;
        [Tooltip("Time interval in seconds between modifying health.")]
        [Min(0)]
        public float interval;
    }

    public enum HealthModifiers { damage, heal }
}
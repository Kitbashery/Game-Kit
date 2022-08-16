using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kitbashery.Gameplay
{
    [HelpURL("https://kitbashery.com/docs/game-kit/damage-zone.html")]
    [AddComponentMenu("Kitbashery/Gameplay/Damage Zone")]
    public class DamageZone : CollisionEvents
    {
        #region Properties:

        [Header("Damage Zone:")]
        [Space]
        [Tooltip("Damage applied per frame.")]
        public int damage = 1;

        [HideInInspector]
        public List<Health> targetHP = new List<Health>();

        /// <summary>
        /// The last health component registered/unregisterd.
        /// </summary>
        [HideInInspector]
        public Health[] lastHealth;

        #endregion

        private void OnValidate()
        {
#if UNITY_EDITOR
            //Autofill events: 
            if (enterEvent == null)
            {
                enterEvent = new UnityEvent();
            }
            if (enterEvent.GetPersistentEventCount() == 0 || EventContainsListenerWithMethod(enterEvent, "RegisterTarget") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(enterEvent, RegisterTarget);
            }

            if (exitEvent == null)
            {
                exitEvent = new UnityEvent();
            }
            if (exitEvent.GetPersistentEventCount() == 0 || EventContainsListenerWithMethod(exitEvent, "UnregisterTarget") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(exitEvent, UnregisterTarget);
            }

            if (stayEvent == null)
            {
                stayEvent = new UnityEvent();
            }
            if (stayEvent.GetPersistentEventCount() == 0 || EventContainsListenerWithMethod(stayEvent, "ApplyDamage") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(stayEvent, ApplyDamage);
            }
#endif
        }

        #region Core Functions:

        /// <summary>
        /// Damage is applied per frame to all <see cref="Health"/> components in the target list.
        /// </summary>
        public void ApplyDamage()
        {
            if (targetHP.Count > 0)
            {
                foreach(Health hp in targetHP)
                {
                    hp.ApplyDamage(damage);
                }
            }
        }

        /// <summary>
        /// Tries to get all the <see cref="Health"/> components of the last collider that made contact and add it to the target list.
        /// </summary>
        public void RegisterTarget()
        {
            lastHealth = lastContact.gameObject.GetComponents<Health>();
            if (lastHealth.Length > 0)
            {
                foreach(Health hp in lastHealth)
                {
                    targetHP.Add(hp);
                }
            }
        }

        /// <summary>
        /// Tries to get all the <see cref="Health"/> components of the last collider that left contact and removes it from the target list.
        /// </summary>
        public void UnregisterTarget()
        {
            lastHealth = lastContact.gameObject.GetComponents<Health>();
            if(lastHealth.Length > 0)
            {
                foreach (Health hp in lastHealth)
                {
                    targetHP.Remove(hp);
                }
            }
        }

        /// <summary>
        /// Adds the required listeners to the <see cref="UnityEvent"/>s in the base class. Call if adding this component via script.
        /// </summary>
        public void AddListeners()
        {
            enterEvent.AddListener(RegisterTarget);
            exitEvent.AddListener(UnregisterTarget);
            stayEvent.AddListener(ApplyDamage);
        }

        #endregion
    }
}
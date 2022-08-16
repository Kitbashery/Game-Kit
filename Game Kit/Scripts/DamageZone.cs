using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kitbashery.core
{
    [AddComponentMenu("Kitbashery/Gameplay/Damage Zone")]
    public class DamageZone : CollisionEvents
    {
        #region Variables:

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
            //Autofill events: 
            if (enterEvent.GetPersistentEventCount() == 0 || EventContainsListenerWithMethod(enterEvent, "RegisterTarget") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(enterEvent, RegisterTarget);
            }

            if (exitEvent.GetPersistentEventCount() == 0 || EventContainsListenerWithMethod(exitEvent, "UnregisterTarget") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(exitEvent, UnregisterTarget);
            }

            if (stayEvent.GetPersistentEventCount() == 0 || EventContainsListenerWithMethod(stayEvent, "ApplyDamage") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(stayEvent, ApplyDamage);
            }
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

        /// <summary>
        /// Used to check if a <see cref="UnityEvent"/> contains a listener with a certain method.
        /// </summary>
        /// <param name="uEvent">The <see cref="UnityEvent"/> to check for a listener in.</param>
        /// <param name="methodName">The name of the method to check for.</param>
        /// <returns>true if an event contains a listener with methodName</returns>
        public bool EventContainsListenerWithMethod(UnityEvent uEvent, string methodName)
        {
            for (int i = 0; i < uEvent.GetPersistentEventCount(); i++)
            {
                if (uEvent.GetPersistentMethodName(i) == methodName)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
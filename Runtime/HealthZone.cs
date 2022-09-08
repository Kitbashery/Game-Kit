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
    /// Modifies the <see cref="Health"/> components of colliding <see cref="GameObject"/>s.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/game-kit/health-zone.html")]
    [AddComponentMenu("Kitbashery/Gameplay/Health Zone")]
    public class HealthZone : CollisionEvents
    {
        #region Properties:

        [Header("Health Zone:")]
        [Space]
        public HealthModifiers modifier = HealthModifiers.damage;
        [Tooltip("Amount of health to modify per frame.")]
        [Min(0)]
        public int amount = 1;

        /// <summary>
        /// Health components to apply damage to.
        /// </summary>
        [HideInInspector]
        public List<Health> targetHP = new List<Health>();

        /// <summary>
        /// The last health components registered/unregisterd.
        /// </summary>
        [HideInInspector]
        public Health lastHealth;

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            //Autofill events: 
            if (enterEvent == null)
            {
                enterEvent = new UnityEvent();
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(enterEvent, RegisterTarget);
            }
            else if(EventContainsListenerWithMethod(enterEvent, "RegisterTarget") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(enterEvent, RegisterTarget);
            }

            if (exitEvent == null)
            {
                exitEvent = new UnityEvent();
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(exitEvent, UnregisterTarget);
            }
            else if(EventContainsListenerWithMethod(exitEvent, "UnregisterTarget") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(exitEvent, UnregisterTarget);
            }

            if (stayEvent == null)
            {
                stayEvent = new UnityEvent();
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(stayEvent, ModifyHealth);
            }
            else if (EventContainsListenerWithMethod(stayEvent, "ModifyHealth") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(stayEvent, ModifyHealth);
            }
        }
#endif

        #region Core Functions:

        /// <summary>
        /// Health change is updated per frame to all <see cref="Health"/> components in the target list.
        /// </summary>
        public void ModifyHealth()
        {
            if (targetHP.Count > 0)
            {
                foreach(Health hp in targetHP)
                {
                    hp.ModifyHealth(modifier, amount);
                }
            }
        }

        /// <summary>
        /// Tries to get the <see cref="Health"/> component of the last collider that made contact and add it to the target list.
        /// </summary>
        public void RegisterTarget()
        {
            lastHealth = lastContact.gameObject.GetComponent<Health>();
            if (lastHealth != null)
            {
                targetHP.Add(lastHealth);
            }
        }

        /// <summary>
        /// Tries to get the <see cref="Health"/> component of the last collider that left contact and removes it from the target list.
        /// </summary>
        public void UnregisterTarget()
        {
            lastHealth = lastContact.gameObject.GetComponent<Health>();
            if(lastHealth != null)
            {
                targetHP.Remove(lastHealth);
            }
        }

        /// <summary>
        /// Adds the required listeners to the <see cref="UnityEvent"/>s in the base class. Call if adding this component via script.
        /// </summary>
        public void AddListeners()
        {
            enterEvent.AddListener(RegisterTarget);
            exitEvent.AddListener(UnregisterTarget);
            stayEvent.AddListener(ModifyHealth);
        }

        #endregion
    }
}
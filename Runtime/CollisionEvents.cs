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
    /// A wrapper for <see cref="MonoBehaviour"/>'s built-in physics methods adding conditional constrains and events.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/game-kit/collision-events.html")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Kitbashery/Event/Collision Events")]
    public class CollisionEvents : MonoBehaviour
    {
        #region Properties:

        [Tooltip("Use trigger events (true/false).")]
        public bool isTrigger = true;
        [Tooltip("Events will not be invoked if there are more collisions taking place than the maximum.")]
        [Min(1)]
        public int maxCollisions = 50;

        [Tooltip("Tags a collider must have in order for events to be invoked.")]
        public List<string> requiredTags = new List<string>();

        /// <summary>
        /// Colliders to ignore when the script is initialized.
        /// </summary>
        [Tooltip("Colliders to ignore (Only supported in Unity 5.5.0b3 and later).")]
        public List<Collider> ignoredColliders = new List<Collider>();
        [Space]

        public UnityEvent enterEvent;
        public UnityEvent exitEvent;
        public UnityEvent stayEvent;

        /// <summary>
        /// The collider that last interacted with the event collider.
        /// </summary>
        [HideInInspector]
        public Collider lastContact;

        /// <summary>
        /// The last collision that occured.
        /// </summary>
        public Collision lastCollision;

        /// <summary>
        /// All colliders currently interacting with the collision event collider.
        /// </summary>
        [HideInInspector]
        public List<Collider> colliders = new List<Collider>();

        /// <summary>
        /// The collider used to detect collision events (Found when Awake() is called).
        /// </summary>
        [HideInInspector]
        public Collider eventCollider;

        #endregion

        #region Initialization & Updates:

        private void Awake()
        {
            eventCollider = transform.GetComponent<Collider>();
            eventCollider.isTrigger = isTrigger;
            IgnoreColliders(ignoredColliders, true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isTrigger == true && colliders.Count < maxCollisions && (requiredTags.Count == 0 || requiredTags.Contains(other.gameObject.tag)))
            {
                lastContact = other;
                if (!colliders.Contains(other))
                {
                    colliders.Add(other);
                }
                enterEvent.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isTrigger == true && colliders.Count < maxCollisions && (requiredTags.Count == 0 || requiredTags.Contains(other.gameObject.tag)))
            {
                lastContact = other;
                if (colliders.Contains(other))
                {
                    colliders.Remove(other);
                }
                exitEvent.Invoke();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (isTrigger == true && (requiredTags.Count == 0 || requiredTags.Contains(other.gameObject.tag)))
            {
                lastContact = other;
                stayEvent.Invoke();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isTrigger == false && colliders.Count < maxCollisions && (requiredTags.Count == 0 || requiredTags.Contains(collision.gameObject.tag)))
            {
                lastContact = collision.collider;
                lastCollision = collision;
                if (!colliders.Contains(collision.collider))
                {
                    colliders.Add(collision.collider);
                }
                enterEvent.Invoke();
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (isTrigger == false && colliders.Count < maxCollisions && (requiredTags.Count == 0 || requiredTags.Contains(collision.gameObject.tag)))
            {
                lastContact = collision.collider;
                lastCollision = collision;
                if (colliders.Contains(collision.collider))
                {
                    colliders.Remove(collision.collider);
                }
                exitEvent.Invoke();
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (isTrigger == false && (requiredTags.Count == 0 || requiredTags.Contains(collision.gameObject.tag)))
            {
                lastContact = collision.collider;
                lastCollision = collision;
                stayEvent.Invoke();
            }
        }

        #endregion

        #region Core Functions:

        /// <summary>
        /// Tells the physics engine to ignore/detect collisions between a list of colliders and the <see cref="eventCollider"/>.
        /// </summary>
        /// <param name="ignoreList">Colliders to ignore/detect.</param>
        /// <param name="ignore">Should the colliders in the ignore list be ignored or detected? (true/false)</param>
        public void IgnoreColliders(List<Collider> ignoreList, bool ignore)
        {
            //Note: Prior to Unity 5.5.0b3 calling this could reset the trigger state of a collider in PhysX.
            //See: https://forum.unity.com/threads/physics-ignorecollision-that-does-not-reset-trigger-state.340836/

#if UNITY_5_5_OR_NEWER
            foreach (Collider col in ignoredColliders)
            {
                Physics.IgnoreCollision(eventCollider, col, ignore);
            }
#else

//Do nothing... 
//TODO: is backward compatibility to support triggers possible?

#endif
        }

        /// <summary>
        /// Used in the Unity editor to check if a <see cref="UnityEvent"/> contains a listener with a certain method.
        /// </summary>
        /// <param name="uEvent">The <see cref="UnityEvent"/> to check for a listener in.</param>
        /// <param name="methodName">The name of the method to check for.</param>
        /// <returns>true if an event contains a listener with methodName.</returns>
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

        public void DebugCollisionCount()
        {
            Debug.LogFormat(gameObject, "|CollisionEvent|: Colliding with {0} colliders.", colliders.Count);
        }

        public void DebugLastCollision()
        {
            Debug.LogFormat(gameObject, "|CollisionEvent|: last collided with {0}'s collider.", lastContact.gameObject.name);
        }

        #endregion
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kitbashery.Gameplay
{
    /// <summary>
    /// Applys force to <see cref="Rigidbody"/>s within it's bounds.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/game-kit/force-field.html")]
    [AddComponentMenu("Kitbashery/Physics/Force Field")]
    public class ForceField : CollisionEvents
    {
        #region Properties:

        [Header("Force Field:")]
        [HideInInspector]
        public List<Rigidbody> rigidbodies = new List<Rigidbody>();

        public ForceTypes mode = ForceTypes.directional;

        public float force = 10;

        public Vector3 forceDirection = Vector3.forward;

        [Space]
        [Tooltip("WindZone component that the force field syncs to if set to use the wind ForceMode.")]
        public WindZone wind;

        private float windPluseTime = 0;

        #endregion

        private void Start()
        {
            isTrigger = true;
            eventCollider.isTrigger = true;
        }

        #region Initialization & Updates:

#if UNITY_EDITOR
        private void OnValidate()
        {
            //Autofill events: 
            if (enterEvent == null)
            {
                enterEvent = new UnityEngine.Events.UnityEvent();
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(enterEvent, RegisterRigidbody);
            }
            else if (EventContainsListenerWithMethod(enterEvent, "RegisterRigidbody") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(enterEvent, RegisterRigidbody);
            }

            if (exitEvent == null)
            {
                exitEvent = new UnityEngine.Events.UnityEvent();
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(exitEvent, UnregisterRigidbody);
            }
            else if (EventContainsListenerWithMethod(exitEvent, "UnregisterRigidbody") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(exitEvent, UnregisterRigidbody);
            }
        }
#endif


        private void FixedUpdate()
        {
            foreach(Rigidbody rigid in rigidbodies)
            {
                switch(mode)
                {
                    case ForceTypes.explode:

                        rigid.AddExplosionForce(force, transform.position, eventCollider.bounds.extents.x);

                        break;

                    case ForceTypes.wind:

                        if(wind != null)
                        {
                            windPluseTime += Time.fixedDeltaTime;
                            if (windPluseTime >= wind.windPulseFrequency)
                            {
                                if(wind.mode == WindZoneMode.Directional)
                                {
                                    rigid.AddForce(wind.transform.forward * Random.Range(wind.windMain, wind.windMain + wind.windTurbulence) * wind.windPulseMagnitude, ForceMode.Impulse);
                                }
                                else
                                {
                                    rigid.AddForce((wind.transform.position * wind.radius) * Random.Range(wind.windMain, wind.windMain + wind.windTurbulence) * wind.windPulseMagnitude, ForceMode.Force);
                                }
                              
                                windPluseTime = 0;
                            }
                        }
                        else
                        {
                            Debug.LogWarningFormat(gameObject, "|Game Kit|: ForceType is set the wind, however no WindZone was defined. The ForceField will not apply force.");
                        }

                        break;

                    case ForceTypes.directional:

                        rigid.AddForce(forceDirection * force);

                        break;

                    case ForceTypes.implode:

                        rigid.AddForce((transform.position - rigid.transform.position).normalized * force, ForceMode.Force);

                        break;
                }
            }
        }

        #endregion

        public void RegisterRigidbody()
        {
            if(lastContact != null)
            {
                rigidbodies.Add(lastContact.attachedRigidbody);
            }
        }

        public void UnregisterRigidbody()
        {
            rigidbodies.Remove(lastContact.attachedRigidbody);
        }
    }

    public enum ForceTypes { wind, directional, explode, implode }
}
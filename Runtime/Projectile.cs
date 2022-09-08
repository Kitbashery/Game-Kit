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
    /// A physics based projectile.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/game-kit/projectile.html")]
    [AddComponentMenu("Kitbashery/Physics/Projectile")]
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : CollisionEvents
    {
        #region Properties:

        [Header("Projectile:")]
        public Rigidbody rigid;

        [Space]
        [Tooltip("Should force be applied to the hit rigidbody?")]
        public bool applyForceOnImpact = true;
        [Tooltip("Force applied to the rigidbody of hit colliders.")]
        public float impactForce = 1f;
        [Tooltip("Initial velocity of the rigidbody.")]
        public float velocity = 500f;

        [Space]
        [Tooltip("Should health be modified on impact?")]
        public bool modifyHealthOnImpact = false;
        public TimedHealthEffect healthEffect;

        /// <summary>
        /// The last health component registered/unregisterd.
        /// </summary>
        private Health lastHealth;

        [Space]
        [Tooltip("Should the projectile be disabled on impact? (useful for pooling).")]
        public bool disableOnImpact = true;
        [Tooltip("How many times should the projectile be allowed to bounce before it can be disabled?")]
        [Min(0)]
        public int ricochets = 0;
        public UnityEvent onRicochet;
        [Min(0)]
        private int currentRicochets = 0;
        [Tooltip("Should the GameObject be deactivated after lifeTime has elapsed.")]
        public bool useLifeTime = true;
        [Tooltip("How long this projectile will live before being deactivated (in seconds).")]
        public float lifeTime = 10f;
        [Min(0)]
        private float life = 0f;

        [Space]
        public bool seekTarget = false;
        public Transform target;
        [Tooltip("Determines how a target should be selected if target is not initially defined.")]
        public TargetModes targetMode = TargetModes.targetFirst;
        [Tooltip("The layer(s) to search for a target in.")]
        public LayerMask layerMask;
        [Tooltip("How should the projectile interact with trigger colliders?")]
        public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
        /// <summary>
        /// Raycast hits collected when the projectile searches for targets to seek.
        /// </summary>
        [HideInInspector]
        public RaycastHit[] hits;
        [Tooltip("The tag required for a GameObject to be set as a target. (leave blank if you don't need this).")]
        public string targetTag;
        [Tooltip("The range to search for a target in.")]
        public float searchRange = 10f;
        [Tooltip("The speed at which the projectile will travel to the target.")]
        public float seekSpeed = 10f;


        #endregion

        #region Initialziation & Updates:

        private void Awake()
        {
            if(rigid == null)
            {
                rigid = gameObject.GetComponent<Rigidbody>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            //Autofill events: 
            if(enterEvent == null)
            {
                enterEvent = new UnityEngine.Events.UnityEvent();
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(enterEvent, Impact);

                healthEffect.amount = 1;
                healthEffect.times = 1;
            }
            else if (EventContainsListenerWithMethod(enterEvent, "Impact") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(enterEvent, Impact);
            }
        }
#endif

        private void OnEnable()
        {
            if (seekTarget == true && target == null)
            {
                FindBestTarget();
            }

            rigid.velocity = Vector3.zero;
            rigid.AddForce(transform.forward * velocity, ForceMode.Impulse);
        }

        private void OnDisable()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            lastHealth = null;
            target = null;
            life = 0;
            currentRicochets = 0;
        }

        private void Update()
        {
            if(useLifeTime == true)
            {
                life += Time.deltaTime;
                if (life >= lifeTime)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void FixedUpdate()
        {
            if (seekTarget == true && target != null)
            {
                transform.LookAt(target);
                rigid.AddForce((target.position - transform.position).normalized * seekSpeed);
            }
        }

        #endregion

        public void Impact()
        {
            if(lastContact != null)
            {
                if (applyForceOnImpact == true)
                {
                    Vector3 hit = lastContact.ClosestPoint(transform.position);
                    if(lastContact.attachedRigidbody != null)
                    {
                        lastContact.attachedRigidbody.AddForceAtPosition(Vector3.one * impactForce, hit);
                    }
                }

                if (modifyHealthOnImpact == true)
                {
                    lastHealth = lastContact.GetComponent<Health>();
                    if (healthEffect.times > 1)
                    {
                        lastHealth.ModifyHealthOverTime(healthEffect);
                    }
                    else
                    {
                        lastHealth.ModifyHealth(healthEffect.modifier, healthEffect.amount);
                    }
                }
            }

            if(ricochets > 0 && lastCollision != null)
            {
                if(disableOnImpact == true && currentRicochets == ricochets)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    onRicochet.Invoke();
                    rigid.velocity = Vector3.zero;
                    Vector3 richochet = -lastCollision.GetContact(0).normal;
                    transform.LookAt(richochet);
                    rigid.AddForce(richochet * (velocity / currentRicochets), ForceMode.Impulse);
                    currentRicochets++;
                }
            }
            else
            {
                if(disableOnImpact == true)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Call if adding this component via script.
        /// </summary>
        public void AddImpactListener()
        {
            enterEvent.AddListener(Impact);
        }

        /// <summary>
        /// Finds the best target for the projectile to seek to.
        /// </summary>
        public void FindBestTarget()
        {
            Physics.SphereCastNonAlloc(transform.position, searchRange, Vector3.zero, hits, Mathf.Infinity, layerMask, triggerInteraction);
            if (hits.Length > 0)
            {
                Transform bestTarget = null;

                switch (targetMode)
                {
                    case TargetModes.targetFirst:

                        foreach (RaycastHit hit in hits)
                        {
                            if (string.IsNullOrEmpty(targetTag) || hit.transform.gameObject.CompareTag(targetTag) == true)
                            {
                                bestTarget = hit.transform;
                            }
                        }

                        break;

                    case TargetModes.targetNearest:

                        float nearestDistance = Mathf.Infinity;
                        foreach (RaycastHit hit in hits)
                        {
                            if (string.IsNullOrEmpty(targetTag) || hit.transform.gameObject.CompareTag(targetTag) == true)
                            {
                                float distance = Vector3.Distance(transform.position, hit.transform.position);
                                if (distance < nearestDistance)
                                {
                                    nearestDistance = distance;
                                    bestTarget = hit.transform;
                                }
                            }
                        }

                        break;

                    case TargetModes.targetFarthest:

                        float farthestDistance = 0;
                        foreach (RaycastHit hit in hits)
                        {
                            if (string.IsNullOrEmpty(targetTag) || hit.transform.gameObject.CompareTag(targetTag) == true)
                            {
                                float distance = Vector3.Distance(transform.position, hit.transform.position);
                                if (distance > farthestDistance)
                                {
                                    farthestDistance = distance;
                                    bestTarget = hit.transform;
                                }
                            }
                        }

                        break;
                }

                target = bestTarget;
            }
        }
    }

    public enum TargetModes { targetFirst, targetNearest, targetFarthest }
}
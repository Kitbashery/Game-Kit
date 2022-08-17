using UnityEngine;

namespace Kitbashery.Gameplay
{
    [HelpURL("https://kitbashery.com/docs/game-kit/projectile.html")]
    [AddComponentMenu("Kitbashery/Gameplay/Projectile")]
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
        public float impactForce = 1;
        [Tooltip("Initial velocity of the rigidbody.")]
        public float velocity = 500;

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
        [Tooltip("Should the GameObject be deactivated after lifeTime has elapsed.")]
        public bool useLifeTime = true;
        [Tooltip("How long this projectile will live before being deactivated (in seconds).")]
        public float lifeTime = 10;
        [Min(0)]
        private float life = 0;

        #endregion

        #region Initialziation & Updates:

        private void Awake()
        {
            if(rigid == null)
            {
                rigid = gameObject.GetComponent<Rigidbody>();
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            //Autofill events: 
            if(enterEvent == null)
            {
                enterEvent = new UnityEngine.Events.UnityEvent();
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(enterEvent, Impact);
            }
            else if (EventContainsListenerWithMethod(enterEvent, "Impact") == false)
            {
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(enterEvent, Impact);
            }
#endif
        }

        private void OnEnable()
        {
            rigid.velocity = Vector3.zero;
            rigid.AddForce(transform.forward * velocity, ForceMode.Impulse);
        }

        private void OnDisable()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            lastHealth = null;
            life = 0;
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

        #endregion

        public void Impact()
        {
            if(applyForceOnImpact == true)
            {
                Vector3 hit = lastContact.ClosestPoint(transform.position);
                lastContact.attachedRigidbody.AddForceAtPosition(Vector3.one * impactForce, hit);
            }

            if(modifyHealthOnImpact == true)
            {
                lastHealth = lastContact.GetComponent<Health>();
                if(healthEffect.times > 1)
                {
                    lastHealth.ModifyHealthOverTime(healthEffect);
                }
                else
                {
                    lastHealth.ModifyHealth(healthEffect.modifier, healthEffect.amount);
                }
            }

            if (disableOnImpact == true)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Call if adding this component via script.
        /// </summary>
        public void AddImpactListener()
        {
            enterEvent.AddListener(Impact);
        }
    }
}

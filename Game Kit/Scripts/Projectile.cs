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
        [Tooltip("Should damage be applied on impact?")]
        public bool damageOnImpact = false;
        [Tooltip("The damage to apply on impact.")]
        [Min(1)]
        public int damage = 1;

        [Space]
        [Tooltip("Should the projectile be disabled on impact? (useful for pooling).")]
        public bool disableOnImpact = true;
        [Tooltip("Should the GameObject be deactivated after lifeTime has elapsed.")]
        public bool useLifeTime = true;
        [Tooltip("How long this projectile will live before being deactivated (in seconds).")]
        public float lifeTime = 10;
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
            }
            if (enterEvent != null)
            {
                if (enterEvent.GetPersistentEventCount() == 0 || EventContainsListenerWithMethod(enterEvent, "Impact") == false)
                {
                    UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(enterEvent, Impact);
                }
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

            if(damageOnImpact == true)
            {
                Health hp = lastContact.GetComponent<Health>();
                if(hp != null)
                {
                    hp.ApplyDamage(damage);
                }
            }

            if(disableOnImpact == true)
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
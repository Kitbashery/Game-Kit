using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kitbashery.core
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        public int damage = 1;
        public float impactForce = 1;
        public float velocity = 500;
        public float lifeTime = 10;
        private float life = 0;
        public Rigidbody rigid;

        [Tooltip("The tag a hit collider is required to have.")]
        public string requiredTag = "HitBox";

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
            life += Time.deltaTime;
            if (life >= lifeTime)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider != null)
            {
                if (collision.collider.gameObject.tag == requiredTag)
                {
                    collision.collider.gameObject.GetComponent<Health>().ApplyDamage(damage);
                }
                else
                {
                    if (collision.rigidbody != null)
                    {
                        Vector3 hit = collision.collider.ClosestPoint(transform.position);
                        collision.rigidbody.AddForceAtPosition(Vector3.one * impactForce, hit);
                    }
                }
                // Debug.Log(collision.collider.gameObject.name);
                gameObject.SetActive(false);
            }
        }
    }
}
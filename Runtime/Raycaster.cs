using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kitbashery.Gameplay
{
    public class Raycaster : MonoBehaviour
    {
        #region Properties:

        public bool canCast = true;

        public RaycastTypes castType = RaycastTypes.single;

        [Min(1)]
        public int rayCount = 1;

        private Ray tempRay;

        [HideInInspector]
        public RaycastHit[] hits;

        public float maxRayDistance = 0;

        public LayerMask layerMask;

        public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        public bool applyForce = false;


        [Min(0)]
        public float force;

        public ForceMode forceMode = ForceMode.Impulse;

        public bool modifyHealth = false;

        public TimedHealthEffect healthEffect;

        public bool canSpawn = false;

        public RaycastSpawn spawn;

        [HideInInspector]
        public GameObject lastSpawned;

        [HideInInspector]
        public Collider lastContact;

        [HideInInspector]
        public Health lastHealth;

        public UnityEvent afterSpawn;

        #endregion

        #region Initialization & Updates:

        private void FixedUpdate()
        {
            if (canCast == true)
            {
                Raycast();
            }
        }

        private void OnDrawGizmosSelected()
        {
            switch(castType)
            {
                case RaycastTypes.single:

                    break;

                case RaycastTypes.sphere:

                    break;

                case RaycastTypes.scatter:

                    break;

                case RaycastTypes.line:

                    break;

                case RaycastTypes.grid:

                    break;

                case RaycastTypes.fan:

                    break;

                case RaycastTypes.circle:

                    break;
            }
        }

        #endregion

        #region Methods:

        public void Raycast()
        {
            switch (castType)
            {
                case RaycastTypes.single:

                    Physics.RaycastNonAlloc(tempRay, hits, maxRayDistance, layerMask);

                    break;

                case RaycastTypes.sphere:
             

                    break;

                case RaycastTypes.scatter:

                    break;

                case RaycastTypes.line:

                    break;

                case RaycastTypes.grid:


                    break;

                case RaycastTypes.circle:

                    break;
            }

            if (hits.Length > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != null)
                    {
                        lastContact = hit.collider;

                        // Apply Force:
                        if(applyForce == true && force > 0) 
                        {
                            lastContact.attachedRigidbody.AddForceAtPosition(hit.normal * force, hit.point, forceMode);
                        }

                        // Spawn:
                        if(canSpawn == true)
                        {
                            if (ObjectPools.Instance != null)
                            {
                                if (Vector3.Angle(hit.point, hit.normal) < spawn.minAngle)
                                {
                                    if (!string.IsNullOrEmpty(spawn.poolID.prefabName))
                                    {
                                        ObjectPools.Instance.GetPooledObject(spawn.poolID.prefabName);
                                    }
                                    else
                                    {
                                        ObjectPools.Instance.GetPooledObject(spawn.poolID.poolIndex);
                                    }

                                    if (lastSpawned != null)
                                    {
                                        lastSpawned.transform.SetPositionAndRotation(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                                        lastSpawned.SetActive(true);
                                    }
                                    else
                                    {
                                        Debug.LogWarning("|Raycaster|: Failed to spawn, you may need to increase a pool's max spawn amount.", gameObject);
                                    }
                                }

                                afterSpawn.Invoke();
                            }
                            else
                            {
                                Debug.LogWarning("There is not an instance of ObjectPools in the scene. Disabling raycaster's spawn functionality.", gameObject);
                                canSpawn = false;
                            }
                        }

                        // Modify Health:
                        if(modifyHealth == true)
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
                }
            }
        }

        #endregion

        [Serializable]
        public struct RaycastSpawn
        {
            public PoolID poolID;
            [Range(0, 360)]
            public float minAngle;
            [Range(0, 360)]
            public float maxAngle;
        }

        public enum RaycastTypes { single, grid, scatter, line, circle, sphere, fan }

    }
}

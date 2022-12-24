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

        private Ray tempRay = new Ray(Vector3.zero, Vector3.forward);

        [HideInInspector]
        public RaycastHit[] hits;

        [Tooltip("The max distance of a ray, if left at 0 will default to infinity.")]
        public float maxRayDistance = 0;
        public float spacing = 1f;
        [Range(0, 360)]
        public float scatterVerticalRange = 45;
        [Range(0, 360)]
        public float scatterHorizontalRange = 45;

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

        private void Awake()
        {
            if(maxRayDistance == 0)
            {
                Debug.Log("|Raycaster|: maxRayDistance is 0 setting it to infinity.");
                maxRayDistance = Mathf.Infinity;
            }
        }

        private void FixedUpdate()
        {
            if (canCast == true)
            {
                Raycast();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            switch (castType)
            {
                case RaycastTypes.single:
                    Gizmos.DrawRay(tempRay.origin, tempRay.direction * maxRayDistance);
                    break;

                case RaycastTypes.scatter:

                    for (int i = 0; i < rayCount; i++)
                    {
                        Vector3 scatterDirection = transform.forward;
                        scatterDirection = Quaternion.Euler(UnityEngine.Random.Range(-scatterVerticalRange, scatterVerticalRange), UnityEngine.Random.Range(-scatterHorizontalRange, scatterHorizontalRange), 0) * scatterDirection;
                        tempRay = new Ray(transform.position, scatterDirection);
                        Gizmos.DrawRay(tempRay.origin, tempRay.direction * maxRayDistance);
                    }
                    break;

                case RaycastTypes.line:
                    for (int i = 0; i < rayCount; i++)
                    {
                        Vector3 lineDirection = transform.forward;
                        tempRay = new Ray(transform.position + new Vector3((i - (rayCount - 1) / 2f) * spacing, 0, 0), lineDirection);
                        Gizmos.DrawRay(tempRay.origin, tempRay.direction * maxRayDistance);
                    }
                    break;

                case RaycastTypes.grid:

                    int gridSize = (int)Mathf.Sqrt(rayCount);
                    int actualRayCount = gridSize * gridSize;
                    hits = new RaycastHit[actualRayCount];
                    for (int i = 0; i < gridSize; i++)
                    {
                        for (int j = 0; j < gridSize; j++)
                        {
                            Vector3 gridDirection = transform.forward;
                            tempRay = new Ray(transform.position + new Vector3((i - (gridSize - 1) / 2f) * spacing, (j - (gridSize - 1) / 2f) * spacing, 0), gridDirection);
                            Gizmos.DrawRay(tempRay.origin, tempRay.direction * maxRayDistance);
                        }
                    }
                        break;

                case RaycastTypes.circle:

                    float angleStep = 360f / rayCount;
                    Quaternion forwardRotation = Quaternion.LookRotation(transform.up, transform.forward);
                    for (int i = 0; i < rayCount; i++)
                    {
                        Vector3 circleDirection = Quaternion.Euler(0, i * angleStep, 0) * transform.right;
                        Vector3 circlePosition = transform.position + forwardRotation * circleDirection * rayCount * spacing;
                        tempRay = new Ray(circlePosition, transform.forward);
                        Gizmos.DrawRay(tempRay.origin, tempRay.direction * maxRayDistance);
                    }
                        break;

                case RaycastTypes.fan:
                    for (int i = 0; i < rayCount; i++)
                    {
                        float angle = 180f / rayCount * i - 90f;
                        angle = Mathf.Clamp(angle, -180f, 180f);
                        Vector3 fanDirection = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
                        tempRay = new Ray(transform.position, fanDirection);
                        Gizmos.DrawRay(tempRay.origin, tempRay.direction * maxRayDistance);
                    }
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

                case RaycastTypes.scatter:

                    hits = new RaycastHit[rayCount];
                    for (int i = 0; i < rayCount; i++)
                    {
                        Vector3 scatterDirection = transform.forward;
                        scatterDirection = Quaternion.Euler(UnityEngine.Random.Range(-scatterVerticalRange, scatterVerticalRange), UnityEngine.Random.Range(-scatterHorizontalRange, scatterHorizontalRange), 0) * scatterDirection;
                        tempRay = new Ray(transform.position, scatterDirection);
                        Physics.RaycastNonAlloc(tempRay, hits, maxRayDistance, layerMask, triggerInteraction);
                    }

                    break;

                case RaycastTypes.line:

                    hits = new RaycastHit[rayCount];
                    for (int i = 0; i < rayCount; i++)
                    {
                        Vector3 lineDirection = transform.forward;
                        tempRay = new Ray(transform.position + new Vector3((i - (rayCount - 1) / 2f) * spacing, 0, 0), lineDirection);
                        Physics.RaycastNonAlloc(tempRay, hits, maxRayDistance, layerMask, triggerInteraction);
                    }
                    break;

                case RaycastTypes.grid:

                    int gridSize = (int)Mathf.Sqrt(rayCount);
                    int actualRayCount = gridSize * gridSize;
                    hits = new RaycastHit[actualRayCount];
                    for (int i = 0; i < gridSize; i++)
                    {
                        for (int j = 0; j < gridSize; j++)
                        {
                            Vector3 gridDirection = transform.forward;
                            tempRay = new Ray(transform.position + new Vector3((i - (gridSize - 1) / 2f) * spacing, (j - (gridSize - 1) / 2f) * spacing, 0), gridDirection);
                            Physics.RaycastNonAlloc(tempRay, hits, maxRayDistance, layerMask, triggerInteraction);
                        }
                    }

                    break;

                case RaycastTypes.circle:

                    hits = new RaycastHit[rayCount];
                    float angleStep = 360f / rayCount;
                    Quaternion forwardRotation = Quaternion.LookRotation(transform.up, transform.forward);
                    for (int i = 0; i < rayCount; i++)
                    {
                        Vector3 circleDirection = Quaternion.Euler(0, i * angleStep, 0) * transform.right;
                        Vector3 circlePosition = transform.position + forwardRotation * circleDirection * rayCount * spacing;
                        tempRay = new Ray(circlePosition, transform.forward);
                        Physics.RaycastNonAlloc(tempRay, hits, maxRayDistance, layerMask, triggerInteraction);
                    }

                    break;

                case RaycastTypes.fan:
                    hits = new RaycastHit[rayCount];
                    for (int i = 0; i < rayCount; i++)
                    {
                        float angle = 180f / rayCount * i - 90f;
                        angle = Mathf.Clamp(angle, -180f, 180f);
                        Vector3 fanDirection = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
                        tempRay = new Ray(transform.position, fanDirection);
                        Physics.RaycastNonAlloc(tempRay, hits, maxRayDistance, layerMask, triggerInteraction);
                    }
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
                        if (applyForce == true && force > 0)
                        {
                            lastContact.attachedRigidbody.AddForceAtPosition(hit.normal * force, hit.point, forceMode);
                        }

                        // Spawn:
                        if (canSpawn == true)
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
                        if (modifyHealth == true)
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

        public enum RaycastTypes { single, grid, scatter, line, circle, fan }
    }
}

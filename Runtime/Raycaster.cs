using System;
using System.Collections;
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
    /// Casts rays and outputs an array of <see cref="RaycastHit"/>s
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/game-kit/raycaster.html")]
    [AddComponentMenu("Kitbashery/Gameplay/Raycaster")]
    public class Raycaster : MonoBehaviour
    {
        #region Properties:

        [Tooltip("If true the raycaster will cast rays ever fixed framerate frame.")]
        public bool canCast = true;

        public RaycastTypes castType = RaycastTypes.single;

        [field: SerializeField, Min(1) ]
        public int rayCount { get; set; } = 1;

        private Ray tempRay = new Ray(Vector3.zero, Vector3.forward);

        [HideInInspector]
        public RaycastHit[] hits;

        [Tooltip("The max distance of a ray, if left at 0 will default to infinity.")]
        public float maxRayDistance = 0;
        [field: SerializeField, Tooltip("Spacing for grid, line and circle cast types.")]
        public float spacing { get; set; } = 1f;

        [field: SerializeField, Range(0, 360), Space]
        public float scatterVerticalRange { get; set; } = 45;
        [field: SerializeField, Range(0, 360)]
        public float scatterHorizontalRange { get; set; } = 45;

        public LayerMask layerMask;

        public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Space]
        [Tooltip("Events that can be invoked when a raycast hits something. (invoked before any force, health or spawn events.")]
        public List<RaycastEvent> raycastEvents = new List<RaycastEvent>();

        [field: SerializeField, Header("Force:")]
        public bool applyForce { get; set; } = false;

        [field: SerializeField, Min(0)]
        public float force { get; set; }

        public ForceMode forceMode = ForceMode.Impulse;

        [field: SerializeField, Header("Health:")]
        public bool modifyHealth { get; set; } = false;

        public TimedHealthEffect healthEffect;

        [field: SerializeField, Header("Spawning:")]
        public bool canSpawn { get; set; } = false;

        public RaycastSpawn spawn;

        [HideInInspector]
        public GameObject lastSpawned;

        [HideInInspector]
        public Collider lastContact;

        [HideInInspector]
        public Health lastHealth;

        public UnityEvent afterSpawn;

        private Transform myTransform;

        #endregion

        #region Initialization & Updates:

        private void Awake()
        {
            myTransform = transform;
            if(maxRayDistance == 0)
            {
                Debug.Log("|Raycaster|: maxRayDistance is 0, setting it to infinity.", gameObject);
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

                    tempRay.origin = myTransform.position;
                    tempRay.direction = myTransform.forward;
                    Physics.RaycastNonAlloc(tempRay, hits, maxRayDistance, layerMask);

                    break;

                case RaycastTypes.scatter:

                    hits = new RaycastHit[rayCount];
                    for (int i = 0; i < rayCount; i++)
                    {
                        tempRay.origin = myTransform.position;
                        tempRay.direction = Quaternion.Euler(UnityEngine.Random.Range(-scatterVerticalRange, scatterVerticalRange), UnityEngine.Random.Range(-scatterHorizontalRange, scatterHorizontalRange), 0) * myTransform.forward;
                        Physics.RaycastNonAlloc(tempRay, hits, maxRayDistance, layerMask, triggerInteraction);
                    }

                    break;

                case RaycastTypes.line:

                    hits = new RaycastHit[rayCount];
                    for (int i = 0; i < rayCount; i++)
                    {
                        tempRay.origin = myTransform.position + (Vector3.right * (i - (rayCount - 1) / 2f) * spacing);
                        tempRay.direction = myTransform.forward;
                        Physics.RaycastNonAlloc(tempRay, hits, maxRayDistance, layerMask, triggerInteraction);
                    }
                    break;

                case RaycastTypes.grid:

                    int gridSize = (int)Mathf.Sqrt(rayCount);
                    hits = new RaycastHit[gridSize * gridSize];
                    for (int i = 0; i < gridSize; i++)
                    {
                        for (int j = 0; j < gridSize; j++)
                        {
                            tempRay.origin = myTransform.position + new Vector3((i - (gridSize - 1) / 2f) * spacing, (j - (gridSize - 1) / 2f) * spacing, 0);
                            tempRay.direction = myTransform.forward;
                            Physics.RaycastNonAlloc(tempRay, hits, maxRayDistance, layerMask, triggerInteraction);
                        }
                    }

                    break;

                case RaycastTypes.circle:

                    hits = new RaycastHit[rayCount];
                    for (int i = 0; i < rayCount; i++)
                    {
                        tempRay.origin = myTransform.position + Quaternion.LookRotation(myTransform.up, myTransform.forward) * Quaternion.Euler(0, i * (360f / rayCount), 0) * myTransform.right * rayCount * spacing;
                        tempRay.direction = myTransform.forward;
                        Physics.RaycastNonAlloc(tempRay, hits, maxRayDistance, layerMask, triggerInteraction);
                    }

                    break;

                case RaycastTypes.fan:
                    hits = new RaycastHit[rayCount];
                    for (int i = 0; i < rayCount; i++)
                    {
                        tempRay.origin = myTransform.position;
                        tempRay.direction = Quaternion.AngleAxis(Mathf.Clamp(180f / rayCount * i - 90f, -180f, 180f), myTransform.up) * myTransform.forward;
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

                        //Invoke events:
                        if(raycastEvents.Count > 0)
                        {
                            foreach (RaycastEvent raycastEvent in raycastEvents)
                            {
                                if(raycastEvent.requiredTag != string.Empty && hit.collider.tag.Contains(raycastEvent.requiredTag) == true)
                                {
                                    raycastEvent.hitEvent.Invoke();
                                }
                            }
                        }

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

        /// <summary>
        /// Sets the value of castType: single = 0, grid = 1, scatter = 2, line = 3, circle = 4, fan = 5 
        /// </summary>
        /// <param name="rayType">An integer value of an enumeration entry defined in RaycastTypes.</param>
        public void SetCastType(int rayType)
        {
            castType = (RaycastTypes)Mathf.Clamp(rayType, 0, 5);
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

        [Serializable]
        public struct RaycastEvent
        {
            [Tooltip("The tag required for the hit event to be invoked.")]
            public string requiredTag;
            [Tooltip("The event to invoke when an object with the required tag is hit.")]
            public UnityEvent hitEvent;
        }

        public enum RaycastTypes { single = 0, grid = 1, scatter = 2, line = 3, circle = 4, fan = 5 }
    }
}

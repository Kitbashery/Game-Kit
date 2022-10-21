using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kitbashery.Gameplay
{
    public class Raycaster : MonoBehaviour
    {
        public RaycastTypes castType = RaycastTypes.single;

        [Min(1)]
        public int rayCount = 1;

        public float maxRayDistance = 0;

        public LayerMask layerMask;

        public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        public bool applyForce = false;

        public RaycastHit[] hits;

        private Ray tempRay;

        public List<RaycastSpawn> spawns = new List<RaycastSpawn>();

        public bool canCast = true;

        private void FixedUpdate()
        {
            if(canCast == true)
            {
                Raycast();
            }
        }

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
        }

        public void Spawn()
        {
            if(hits.Length > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != null)
                    {
                        if (Vector3.Angle(hit.point, hit.normal) < spawns[0].minAngle)
                        {

                        }
                    }
                }
            }
        }
    }

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

    public enum RaycastEvents { spawn, spawnRandom, applyForce, damage, heal }

}
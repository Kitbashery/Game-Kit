using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kitbashery.core
{
    /// <summary>
    /// Enables GameObjects pooled by <see cref="ObjectPools"/> in sequential waves.
    /// </summary>
    [AddComponentMenu("Kitbashery/Gameplay/Spawner")]
    public class Spawner : MonoBehaviour
    {
        #region Variables:

        public bool canSpawn = false;

        [Tooltip("Spawned GameObjects will be positioned in the direction the spawner is facing.")]
        public bool spawnForward = false;
        [Tooltip("How far forward to spawn a GameObject (if Spawn Forward is checked).")]
        public float forwardDistance = 1;
        public Vector3 spawnOffset;
        [Tooltip("Sequential information on what pool to spawn GameObjects from, how often and how many.")]
        public List<Wave> waves = new List<Wave>();

        [HideInInspector]
        public GameObject lastSpawned;

        [HideInInspector]
        [Min(0)]
        public int currentWave = 0;

        [HideInInspector]
        [Min(0)]
        public int currentWaveSpawns = 0;

        private float currentTime = 0;

        #endregion

        #region Initialization & Updates:

        // Update is called once per frame.
        void Update()
        {
            if (canSpawn == true)
            {
                if (waves.Count > 0)
                {
                    if (currentWave > waves.Count)
                    {
                        currentWave = 0;
                    }
                    else
                    {
                        currentTime += Time.deltaTime;
                        if (currentTime >= waves[currentWave].interval)
                        {
                            if (!string.IsNullOrEmpty(waves[currentWave].prefabName))
                            {
                                lastSpawned = ObjectPools.Instance.GetPooledObject(waves[currentWave].prefabName);
                            }
                            else
                            {
                                lastSpawned = ObjectPools.Instance.GetPooledObject(waves[currentWave].poolIndex);
                            }

                            if (lastSpawned != null)
                            {
                                lastSpawned.transform.rotation = transform.rotation;
                                if (spawnForward == true)
                                {
                                    lastSpawned.transform.position = transform.TransformPoint((transform.forward * forwardDistance) + spawnOffset);
                                }
                                else
                                {
                                    lastSpawned.transform.position += spawnOffset;
                                }

                                lastSpawned.SetActive(true);
                                waves[currentWave].onSpawn.Invoke();
                                currentWaveSpawns++;
                                if (currentWaveSpawns == waves[currentWave].amount)
                                {
                                    currentWaveSpawns = 0;
                                    currentWave++;
                                    if (currentWave == waves.Count)
                                    {
                                        currentWave = 0;
                                        canSpawn = false;
                                    }
                                    waves[currentWave].onWaveComplete.Invoke();
                                }
                                currentTime = 0;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("|Spawner|: " + " on " + gameObject.name + " is trying to spawn but has no waves defined.");
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            if (spawnForward == true)
            {
                Gizmos.DrawSphere(transform.TransformPoint((transform.forward * forwardDistance) + spawnOffset), 0.1f);
            }
            else
            {
                Gizmos.DrawSphere(transform.position + spawnOffset, 0.1f);
            }
        }

        private IEnumerator Delay(float time)
        {
            canSpawn = false;
            yield return new WaitForSeconds(time);
            canSpawn = true;
        }

        #endregion

        #region Core Functions:

        public void DelaySpawn(float time)
        {
            StartCoroutine(Delay(time));
        }

        public void ToggleSpawn(bool toggle)
        {
            canSpawn = toggle;
        }

        public void SetWave(int wave)
        {
            if (wave <= waves.Count)
            {
                currentWave = 0;
            }
            else
            {
                Debug.LogWarning("|Spawner|: " + gameObject.name + " tried to set the wave beyond the maximum amount of waves.");
            }

        }

        public void DebugCurrentWave()
        {
            Debug.Log("|Spawner|: " + gameObject.name + " is currently on wave " + currentWave + " of " + waves.Count + " and has spawned " + currentWaveSpawns + " of " + waves[currentWave].amount + " times.");
        }

        #endregion
    }

    /// <summary>
    /// Defines spawns used by <see cref="Spawner"/> to enable pooled GameObjects.
    /// </summary>
    [Serializable]
    public struct Wave
    {
        [Tooltip("The index of a pool in the ObjectPools instance pool list.")]
        [Min(0)]
        public int poolIndex;

        [Tooltip("If specified the spawner will try to get GameObjects from the pool by the prefab name instead of index.")]
        public string prefabName;

        [Tooltip("The amount of objects to spawn. (Make sure this amount is less or equal to the pool's prefab amount).")]
        [Min(0)]
        public int amount;

        [Tooltip("Time in seconds between spawns.")]
        [Min(0)]
        public float interval;

        [Space]
        public UnityEvent onSpawn;

        public UnityEvent onWaveComplete;
    }
}
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
    /// Enables GameObjects pooled by <see cref="ObjectPools"/> in sequential waves.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/game-kit/spawner.html")]
    [AddComponentMenu("Kitbashery/Gameplay/Spawner")]
    public class Spawner : MonoBehaviour
    {
        #region Properties:

        [field: SerializeField]
        public bool canSpawn { get; set; }

        [field: SerializeField, Tooltip("Spawned GameObjects will be positioned in the direction the spawner is facing.")]
        public bool spawnForward { get; set; } = false;
        [field: SerializeField, Tooltip("How far forward to spawn a GameObject (if Spawn Forward is checked).")]
        public float forwardDistance { get; set; } = 0;
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

        private void Start()
        {
            if (ObjectPools.Instance == null)
            {
                Debug.LogWarning("There is not an instance of ObjectPools in the scene. Disabling spawner.");
                canSpawn = false;
            }
        }

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
                            if (!string.IsNullOrEmpty(waves[currentWave].poolIdentifiers[currentWaveSpawns].prefabName))
                            {
                                lastSpawned = ObjectPools.Instance.GetPooledObject(waves[currentWave].poolIdentifiers[currentWaveSpawns].prefabName);
                            }
                            else
                            {
                                lastSpawned = ObjectPools.Instance.GetPooledObject(waves[currentWave].poolIdentifiers[currentWaveSpawns].poolIndex);
                            }

                            if (lastSpawned != null)
                            {
                                waves[currentWave].onSpawn.Invoke();

                                lastSpawned.transform.rotation = transform.rotation;
                                if (spawnForward == true)
                                {
                                    lastSpawned.transform.position = transform.TransformPoint((transform.forward * forwardDistance) + spawnOffset);
                                }
                                else
                                {
                                    lastSpawned.transform.position = transform.TransformPoint(spawnOffset);
                                }

                                lastSpawned.SetActive(true);
                            }
                            else
                            {
                                Debug.LogWarning("|Spawner|: Failed to spawn, make sure all wave amounts are less than or equal to the pool's max amount or increase the pool's amount.", gameObject);
                            }

                            currentWaveSpawns++;
                            if (currentWaveSpawns == waves[currentWave].poolIdentifiers.Count)
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
                else
                {
                    Debug.LogWarning("|Spawner|: Trying to spawn but no waves are defined.", gameObject);
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

        public void SetWave(int wave)
        {
            if (wave <= waves.Count)
            {
                currentWave = 0;
            }
            else
            {
                Debug.LogWarning("|Spawner|: Tried to set the wave beyond the maximum amount of waves.", gameObject);
            }

        }

        /// <summary>
        /// Randomizes the spawns of a wave.
        /// </summary>
        /// <param name="wave">The wave to randomize.</param>
        /// <returns>The wave with randomized spawns.</returns>
        public Wave RandomizeWave(Wave wave)
        {
            List<PoolID> ids = new List<PoolID>();
            for(int i = 0; i < wave.spawnAmount; i++)
            {
                PoolID id = new();
                id.poolIndex = wave.randomPools[UnityEngine.Random.Range(0, wave.randomPools.Count)];
                ids.Add(id);
            }
            wave.poolIdentifiers = ids;

            return wave;
        }

        /// <summary>
        /// Randomizes all waves that have randomWaves set to true.
        /// </summary>
        public void RandomizeWaves()
        {
            for(int i = 0; i < waves.Count; i++)
            {
                if (waves[i].randomizeWaves == true)
                {
                    waves[i] = RandomizeWave(waves[i]);
                }
            }
        }

        public void DebugCurrentWave()
        {
            Debug.LogFormat(gameObject, "|Spawner|: Currently on wave {0} of {1} and has spawned {2} of {3} times.", currentWave, waves.Count, currentWaveSpawns, waves[currentWave].poolIdentifiers.Count);
        }

        public void RandomizeSpawnOffset(float radius)
        {
            UnityEngine.Random.InitState(spawnOffset.GetHashCode());
            spawnOffset = UnityEngine.Random.insideUnitSphere * radius;
        }

        #endregion
    }

    /// <summary>
    /// Defines spawns used by <see cref="Spawner"/> to enable pooled GameObjects.
    /// </summary>
    [Serializable]
    public struct Wave
    {
        [Space]
        [Tooltip("Time in seconds between spawns.")]
        [Min(0)]
        public float interval;

        [Tooltip("The amount of spawns in this wave.")]
        public int spawnAmount;

        public UnityEvent onSpawn;

        public UnityEvent onWaveComplete;

        [Tooltip("Defines the prefabs to enable from a pool during this wave.")]
        public List<PoolID> poolIdentifiers;

        [Space]
        [Tooltip("Should spawns be randomized? (used instead of poolIdentifiers).")]
        public bool randomizeWaves;
        [Tooltip("Pool indices to select spawns from.")]
        public List<int> randomPools;
    }
}
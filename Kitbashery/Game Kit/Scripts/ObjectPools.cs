using System;
using System.Collections.Generic;
using UnityEngine;

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
    /// A singleton class for managing multiple pools of <see cref="GameObject"/>s.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/game-kit/object-pools.html")]
    [DisallowMultipleComponent]
    [AddComponentMenu("Kitbashery/Gameplay/Object Pools")]
    public class ObjectPools : MonoBehaviour
    {
        #region Properties:

        public static ObjectPools Instance;

        /// <summary>
        /// A non-reorderable list of <see cref="Pool"/>s.
        /// </summary>
        [NonReorderable]
        public List<Pool> pools = new List<Pool>();

        private Dictionary<string, Pool> namedPools = new Dictionary<string, Pool>();

        /// <summary>
        /// Temporary GameObject used when populating pools.
        /// </summary>
        private GameObject tmp;

        #endregion

        #region Initialization & Updates:

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            CreatePools();
        }

        #endregion

        #region Core Functions:

        public void CreatePools()
        {
            foreach (Pool pool in pools)
            {
                if (!namedPools.ContainsKey(pool.prefab.name))
                {
                    PopulatePool(pool);
                    namedPools.Add(pool.prefab.name, pool);
                }
                else
                {
                    Debug.LogWarningFormat("|Object Pool|: Failed to create pool ({0}) a pool already exisits for that prefab, concider renaming.", pool.prefab.name);
                }
            }
            tmp = null;
        }

        public void PopulatePool(Pool pool)
        {
            if (pool.pooledObjects.Count < pool.amount)
            {
                for (int i = 0; i < pool.amount; i++)
                {
                    tmp = Instantiate(pool.prefab);
                    tmp.SetActive(false);
                    tmp.gameObject.hideFlags = pool.hideFlags;
                    if (pool.sequencialNaming == true)
                    {
                        tmp.gameObject.name = pool.prefab.name + " " + i;
                    }
                    pool.pooledObjects.Add(tmp);
                }
            }
        }

        public void ExpandPool(Pool pool, int amount)
        {
            pool.amount += amount;
            PopulatePool(pool);
        }

        /// <summary>
        /// Destroys all objects in all pools.
        /// </summary>
        /// <param name="omitActive">Preserve pooled GameObjects that are currently active in the scene.</param>
        public void DestroyPools(bool omitActive)
        {
            foreach (Pool pool in pools)
            {
                for (int i = pool.pooledObjects.Count; i > 0; i--)
                {
                    if (omitActive == false)
                    {
                        Destroy(pool.pooledObjects[i]);
                    }
                    else
                    {
                        if (pool.pooledObjects[i].activeSelf == false)
                        {
                            Destroy(pool.pooledObjects[i]);
                        }
                    }
                }

                pool.pooledObjects.Clear();
            }
        }

        public void ActivatePooledObject(int index)
        {
            if (index <= pools.Count)
            {
                for (int i = 0; i < pools[index].amount; i++)
                {
                    if (!pools[index].pooledObjects[i].activeInHierarchy)
                    {
                        pools[index].pooledObjects[i].SetActive(true);
                    }
                }
            }
            else
            {
                Debug.LogWarningFormat("|Object Pool|: failed to activate GameObject from pool {0} GameObject will be null, make sure the index is within range.", index);
            }
        }

        public void ActivatePooledObject(string prefabName)
        {
            if (!string.IsNullOrEmpty(prefabName) && namedPools.ContainsKey(prefabName))
            {
                for (int i = 0; i < namedPools[prefabName].amount; i++)
                {
                    if (!namedPools[prefabName].pooledObjects[i].activeInHierarchy)
                    {
                        namedPools[prefabName].pooledObjects[i].SetActive(true);
                    }
                }
            }
            else
            {
                Debug.LogWarningFormat("|Object Pool|: failed to activate GameObject from pool ({0}) GameObject will be null, make sure the name is correct.", name);
            }
        }

        /// <summary>
        /// Gets a pooled GameObject by the index of its pool.
        /// </summary>
        /// <param name="index">The index of a pool in <see cref="pools"/>.</param>
        /// <returns>The first inactive prefab instance.</returns>
        public GameObject GetPooledObject(int index)
        {
            if (index <= pools.Count)
            {
                for (int i = 0; i < pools[index].amount; i++)
                {
                    if (!pools[index].pooledObjects[i].activeInHierarchy)
                    {
                        return pools[index].pooledObjects[i];
                    }
                }
            }
            else
            {
                Debug.LogWarningFormat("|Object Pool|: failed to get GameObject from pool {0} GameObject will be null, make sure the index is within range.", index);
            }

            return null;
        }

        /// <summary>
        /// Gets a pooled GameObject by its prefab name.
        /// </summary>
        /// <param name="prefabName">The name of the prefab to get an instance of.</param>
        /// <returns>The first inactive prefab instance.</returns>
        public GameObject GetPooledObject(string prefabName)
        {
            if (!string.IsNullOrEmpty(prefabName) && namedPools.ContainsKey(prefabName))
            {
                for (int i = 0; i < namedPools[prefabName].amount; i++)
                {
                    if (!namedPools[prefabName].pooledObjects[i].activeInHierarchy)
                    {
                        return namedPools[prefabName].pooledObjects[i];
                    }
                }
            }
            else
            {
                Debug.LogWarningFormat("|Object Pool|: failed to get GameObject from pool ({0}) GameObject will be null, make sure the name is correct.", name);
            }

            return null;
        }

        #endregion
    }

    [Serializable]
    public struct Pool
    {
        [Tooltip("The GameObject to pool.")]
        public GameObject prefab;
        [Tooltip("The initial amount of GameObjects to instantiate.")]
        [Min(1)]
        public int amount;
        [Tooltip("HideFlags for prefabs instantiated via the pooling system. Useful for hiding pooled objects in the heirarchy.")]
        public HideFlags hideFlags;
        [Tooltip("Use numbered names for GameObjects instead of name(clone).")]
        public bool sequencialNaming;
        [Tooltip("The GameObjects currently pooled.")]
        public List<GameObject> pooledObjects;
    }

    [Serializable]
    public struct PoolID
    {
        [Tooltip("The index of a pool in the ObjectPools instance pool list.")]
        [Min(0)]
        public int poolIndex;

        [Tooltip("If specified the spawner will try to get GameObjects from the pool by the prefab name instead of index.")]
        public string prefabName;
    }
}
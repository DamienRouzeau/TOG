// Created by Ronis Vision. All rights reserved
// 08.08.2020.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RVModules.RVUtilities;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace RVModules.RVCommonGameLibrary.Pooling
{
    /// <summary>
    /// Can be used as normal component object, or singleton
    /// todo make thread safe
    /// </summary>
    public class PoolManager : MonoSingleton<PoolManager>
    {
        #region Fields

        public RvLogger logger = new RvLogger("PoolManager");

        [FormerlySerializedAs("createPoolsOnAwake")]
        public bool createPoolsOnInitialization = true;

        [SerializeField]
        private List<PoolConfig> poolsConfig = new List<PoolConfig>();

        private Dictionary<string, ObjectPool<IPoolable>> pools = new Dictionary<string, ObjectPool<IPoolable>>();

        private bool initialized;

        #endregion

        #region Properties

        public ObjectPool<IPoolable>[] GetAllPoolsAsArray => pools.Values.ToArray();
        public Dictionary<string, ObjectPool<IPoolable>> GetAllPool => pools;

        public bool PoolsCreated { get; private set; }

        #endregion

        #region Public methods

        public bool TryGetPool(string _name, out ObjectPool<IPoolable> _pool) => pools.TryGetValue(_name, out _pool);

//        public bool TryGetPoolByPrefab(GameObject _prefab, out UnityObjectPool _pool)
//        {
//            foreach (var poolsValue in pools)
//            {
//                if (poolsValue.Value.Prefab.name != _prefab.name) continue;
//                _pool = poolsValue.Value;
//                return true;
//            }
//
//            _pool = null;
//            return false;
//        }

        public bool TryGetObject<T>(string _name, out T _object) where T : IPoolable
        {
            if (pools.TryGetValue(_name, out var pool))
            {
                _object = (T) pool.GetObject();
                return true;
            }

            logger.LogInfo($"There is no pool {_name}");
            _object = default;
            return false;
        }

        public bool TryGetObject(string _name, out IPoolable _object)
        {
            if (pools.TryGetValue(_name, out var pool))
            {
                _object = pool.GetObject();
                return true;
            }

            logger.LogInfo($"There is no pool {_name}");
            _object = default;
            return false;
        }

        public T GetObject<T>(string _name) where T : IPoolable
        {
            if (pools.TryGetValue(_name, out var pool)) return (T) pool.GetObject();
            throw new Exception($"There is no pool {_name}");
        }

        public IPoolable GetObject(string _name)
        {
            if (pools.TryGetValue(_name, out var pool)) return pool.GetObject();
            throw new Exception($"There is no pool {_name}");
        }

        /// <summary>
        /// Returns true is succesfully returned object to pool
        /// This should be treated as backup, normally you should just call IPoolable.OnDespawn 
        /// </summary>
        public bool ReturnObject(string poolName, IPoolable poolable)
        {
            if (poolable == null) throw new ArgumentNullException();

            if (pools.TryGetValue(poolName, out var _pool))
            {
                _pool.PutObject(poolable);
                return true;
            }

            logger.LogWarning($"There is no pool for object {poolable}", poolable as Object);
            return false;
        }

        public void CreatePools()
        {
            if (PoolsCreated)
            {
                logger.LogWarning("Pools has already been created!");
                return;
            }

            var sw = Stopwatch.StartNew();
            foreach (var poolConfig in poolsConfig)
            {
                if (poolConfig.prefab == null)
                {
                    logger.LogError($"pool {poolConfig.optionalName} prefab cannot be null!");
                    continue;
                }

                var ipoolable = poolConfig.prefab.GetComponent<IPoolable>();

                if (ipoolable == null)
                {
                    logger.LogError($"prefab {poolConfig.prefab} doesn't have IPoolable component on it!");
                    continue;
                }

                AddNewPool(poolConfig);
            }

            logger.LogInfo($"{name} pool manager initialization: {sw.ElapsedMilliseconds}ms");
            sw.Stop();
            PoolsCreated = true;
        }

        public bool HasPool(string poolName) => pools.ContainsKey(poolName);

        /// <summary>
        /// Creates new pool if it isn't already added, otherwise does nothing
        /// </summary>
        public void CreatePoolIfDoesntExist(PoolConfig _poolConfig)
        {
            if (!HasPool(_poolConfig.PoolName)) AddNewPool(_poolConfig);
        }

        /// <summary>
        /// Creates new pool
        /// </summary>
        public void AddNewPool(PoolConfig poolConfig)
        {
            if (poolConfig.prefab == null)
            {
                logger.LogError("Can't add pool with null prefab!");
                return;
            }

            if (poolConfig.prefab.GetComponent<IPoolable>() == null)
            {
                logger.LogError("Pool prefab doesn't have IPoolable component on it!", poolConfig.prefab);
                return;
            }

            if (pools.ContainsKey(poolConfig.PoolName))
            {
                logger.LogWarning($"Pool with object named {poolConfig.PoolName} already exist!");
                return;
            }

            var newPool = new ObjectPool<IPoolable>(
                () =>
                {
                    var poolableGameObject = Instantiate(poolConfig.prefab, poolConfig.spawnParent);
                    var poolable = poolableGameObject.GetComponent<IPoolable>();

                    IPoolable[] otherPoolables = null;
                    switch (poolConfig.multiplePoolableHandling)
                    {
                        case MultiplePoolableHandling.GetComponents:
                            otherPoolables = poolableGameObject.GetComponents<IPoolable>();
                            break;
                        case MultiplePoolableHandling.GetComponentsInChildren:
                            otherPoolables = poolableGameObject.GetComponentsInChildren<IPoolable>();
                            break;
                    }

                    if (otherPoolables != null) SubscribeToOtherPoolables(otherPoolables, poolable);

                    return poolable;
                });

            void SubscribeToOtherPoolables(IPoolable[] poolables, IPoolable mainPoolable)
            {
                foreach (var component in poolables)
                {
                    if (component == mainPoolable) continue;
                    mainPoolable.OnSpawn += component.OnSpawn;
                    mainPoolable.OnDespawn += component.OnDespawn;
                }
            }

            if (!poolsConfig.Contains(poolConfig)) poolsConfig.Add(poolConfig);

            pools.Add(poolConfig.PoolName, newPool);
        }

        #endregion

        #region Not public methods

        protected override void SingletonInitialization()
        {
            if (initialized) return;
            if (!createPoolsOnInitialization) return;
            initialized = true;

            CreatePools();
        }

        protected override void Awake()
        {
            base.Awake();
            if (initialized) return;
            initialized = true;

            if (createPoolsOnInitialization) CreatePools();
        }

        #endregion
    }
}
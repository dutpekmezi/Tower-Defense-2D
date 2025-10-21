using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Dutpekmezi.Services.PoolService
{
    public class ObjectPoolManager : MonoBehaviour
    {
        [SerializeField] private bool _addToDontDestroyOnLoad;

        private GameObject _emptyHolder;

        private static GameObject _particleSystemsEmpty;
        private static GameObject _gameObjectsEmpty;
        private static GameObject _soundFXEmpty;

        private static Dictionary<GameObject, ObjectPool<GameObject>> _objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        private static Dictionary<GameObject, GameObject> _cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

        public enum PoolType
        {
            ParticleSystems,
            GameObjects,
            SoundFX
        }

        public static PoolType PoolingType;

        private void Awake()
        {
            SetupEmpties();
        }

        private void SetupEmpties()
        {
            _emptyHolder = new GameObject("Object Pools");

            _particleSystemsEmpty = new GameObject("Particle Effects");
            _particleSystemsEmpty.transform.SetParent(_emptyHolder.transform);

            _gameObjectsEmpty = new GameObject("Game Objects");
            _gameObjectsEmpty.transform.SetParent(_gameObjectsEmpty.transform);

            _soundFXEmpty = new GameObject("Sound FX");
            _soundFXEmpty.transform.SetParent(_soundFXEmpty.transform);

            if (_addToDontDestroyOnLoad)
            {
                DontDestroyOnLoad(_particleSystemsEmpty.transform.root);
            }
        }

        private static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType = PoolType.GameObjects)
        {
            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => CreateObject(prefab, pos, rot, poolType),
                actionOnGet: OnGetObject,
                actionOnRelease: OnReleaseObject,
                actionOnDestroy: OnDestroyObject);

            _objectPools.Add(prefab, pool);
        }

        private static void CreatePool(GameObject prefab, Transform parent, Quaternion rot, PoolType poolType = PoolType.GameObjects)
        {
            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => CreateObject(prefab, parent, rot, poolType),
                actionOnGet: OnGetObject,
                actionOnRelease: OnReleaseObject,
                actionOnDestroy: OnDestroyObject);

            _objectPools.Add(prefab, pool);
        }

        private static GameObject CreateObject(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType = PoolType.GameObjects)
        {
            prefab.SetActive(false);

            var instance = Instantiate(prefab, pos, rot);

            prefab.SetActive(true);

            var parent = SetParentObject(poolType);
            instance.transform.SetParent(parent.transform);

            return instance;
        }

        private static GameObject CreateObject(GameObject prefab, Transform parent, Quaternion rot, PoolType poolType = PoolType.GameObjects)
        {
            prefab.SetActive(false);

            var instance = Instantiate(prefab, parent);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = rot;
            instance.transform.localScale = Vector3.one;

            prefab.SetActive(true);

            return instance;
        }

        private static void OnGetObject(GameObject obj)
        {
            // optional
        }

        private static void OnReleaseObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        private static void OnDestroyObject(GameObject obj)
        {
            if (_cloneToPrefabMap.ContainsKey(obj))
            {
                _cloneToPrefabMap.Remove(obj);
            }
        }

        private static GameObject SetParentObject(PoolType pooltype)
        {
            return pooltype switch
            {
                PoolType.GameObjects => _gameObjectsEmpty,
                PoolType.ParticleSystems => _particleSystemsEmpty,
                PoolType.SoundFX => _soundFXEmpty,
                _ => null,
            };
        }

        private static T SpawnObject<T>(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects) where T : Object
        {
            if (!_objectPools.ContainsKey(objectToSpawn))
            {
                CreatePool(objectToSpawn, spawnPos, spawnRotation, poolType);
            }

            GameObject obj = _objectPools[objectToSpawn].Get();

            if (obj != null)
            {
                if (!_cloneToPrefabMap.ContainsKey(obj))
                {
                    _cloneToPrefabMap.Add(obj, objectToSpawn);
                }

                obj.transform.position = spawnPos;
                obj.transform.rotation = spawnRotation;
                obj.SetActive(true);

                if (typeof(T) == typeof(GameObject))
                {
                    return (T)(object)obj;
                }

                T component = obj.GetComponent<T>();
                if (component == null)
                {
                    Debug.LogError($"Object {objectToSpawn.name} doesn't have component of type {typeof(T)}");
                    return default;
                }

                return component;
            }

            return default;
        }

        private static T SpawnObject<T>(GameObject objectToSpawn, Transform parent, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects) where T : Object
        {
            if (!_objectPools.ContainsKey(objectToSpawn))
            {
                CreatePool(objectToSpawn, parent, spawnRotation, poolType);
            }

            GameObject obj = _objectPools[objectToSpawn].Get();

            if (obj != null)
            {
                if (!_cloneToPrefabMap.ContainsKey(obj))
                {
                    _cloneToPrefabMap.Add(obj, objectToSpawn);
                }

                obj.transform.SetParent(parent);
                obj.transform.localPosition = Vector3.zero;
                obj.SetActive(true);

                if (typeof(T) == typeof(GameObject))
                {
                    return (T)(object)obj;
                }

                T component = obj.GetComponent<T>();
                if (component == null)
                {
                    Debug.LogError($"Object {objectToSpawn.name} doesn't have component of type {typeof(T)}");
                    return default;
                }

                return component;
            }

            return default;
        }

        public static T SpawnObject<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects) where T : Component
        {
            return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRotation, poolType);
        }

        public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects)
        {
            return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRotation, poolType);
        }

        public static T SpawnObject<T>(T typePrefab, Transform parent, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects) where T : Component
        {
            return SpawnObject<T>(typePrefab.gameObject, parent, spawnRotation, poolType);
        }

        public static GameObject SpawnObject(GameObject objectToSpawn, Transform parent, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects)
        {
            return SpawnObject<GameObject>(objectToSpawn, parent, spawnRotation, poolType);
        }

        public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObjects)
        {
            if (_cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
            {
                GameObject parentObject = SetParentObject(poolType);

                if (obj.transform.parent != parentObject.transform)
                {
                    obj.transform.SetParent(parentObject.transform);
                }

                if (_objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
                {
                    pool.Release(obj);
                }
            }
            else
            {
                Debug.LogWarning("Trying to return an object that is not pooled: " + obj.name);
            }
        }
    }
}

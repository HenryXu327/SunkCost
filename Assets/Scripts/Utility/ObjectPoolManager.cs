using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Utility
{
    public class ObjectPoolManager : MonoBehaviour
    {
        [SerializeField] private bool addToDontDestroyOnLoad = false;

        private GameObject holder;

        private static GameObject particleSystemHolder;
        private static GameObject gameObjectHolder;

        private static Dictionary<GameObject, ObjectPool<GameObject>> objectPools;
        private static Dictionary<GameObject, GameObject> cloneToPrefabMap;

        public enum PoolType
        {
            ParticleSystem,
            GameObject
        }

        public static PoolType PoolingType;

        private void Awake()
        {
            objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
            cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

            SetupEmpties();
        }

        private void SetupEmpties()
        {
            holder = new GameObject("Object Pools");

            particleSystemHolder = new GameObject("ParticleSystem Pools");
            particleSystemHolder.transform.SetParent(holder.transform);
            gameObjectHolder = new GameObject("GameObject Pools");
            gameObjectHolder.transform.SetParent(holder.transform);

            if (addToDontDestroyOnLoad)
            {
                DontDestroyOnLoad(particleSystemHolder.transform.root);
            }
        }

        private static void CreatePool(GameObject prefab, Vector3 position, Quaternion rotation,
            PoolType poolType = PoolType.GameObject)
        {
            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => CreateObject(prefab, position, rotation, poolType),
                actionOnGet: OnGetObject,
                actionOnRelease: OnReleaseObject,
                actionOnDestroy: OnDestroyObject
            );

            objectPools.Add(prefab, pool);
        }
        
        private static void CreatePool(GameObject prefab, Transform parent, Quaternion rotation,
            PoolType poolType = PoolType.GameObject)
        {
            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => CreateObject(prefab, parent, rotation, poolType),
                actionOnGet: OnGetObject,
                actionOnRelease: OnReleaseObject,
                actionOnDestroy: OnDestroyObject
            );

            objectPools.Add(prefab, pool);
        }

        private static GameObject CreateObject(GameObject prefab, Vector3 position, Quaternion rotation,
            PoolType poolType = PoolType.GameObject)
        {
            prefab.SetActive(false);
            
            GameObject obj = Instantiate(prefab, position, rotation);
            
            prefab.SetActive(true);
            
            GameObject parentObj = SetParent(poolType);
            obj.transform.SetParent(parentObj.transform);
            
            return obj;
        }
        
        private static GameObject CreateObject(GameObject prefab, Transform parent, Quaternion rotation,
            PoolType poolType = PoolType.GameObject)
        {
            prefab.SetActive(false);
            
            GameObject obj = Instantiate(prefab, parent);
            
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = rotation;
            obj.transform.localScale = Vector3.one;
            
            prefab.SetActive(true);
            
            return obj;
        }

        private static void OnGetObject(GameObject obj)
        {
            
        }

        private static void OnReleaseObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        private static void OnDestroyObject(GameObject obj)
        {
            if (cloneToPrefabMap.ContainsKey(obj))
            {
                cloneToPrefabMap.Remove(obj);
            }
        }
        
        private static GameObject SetParent(PoolType poolType)
        {
            switch (poolType)
            {
                case PoolType.ParticleSystem:
                    return particleSystemHolder;
                case PoolType.GameObject:
                    return gameObjectHolder;
                default:
                    return null;
            }
        }

        private static T SpawnObject<T>(GameObject prefab, Vector3 position, Quaternion rotation,
            PoolType poolType = PoolType.GameObject) where T : Object
        {
            if (!objectPools.ContainsKey(prefab))
            {
                CreatePool(prefab, position, rotation, poolType);
            }
            
            GameObject obj = objectPools[prefab].Get();

            if (obj != null)
            {
                if (!cloneToPrefabMap.ContainsKey(obj))
                {
                    cloneToPrefabMap.Add(obj, prefab);
                }
                
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);

                if (typeof(T) == typeof(GameObject))
                {
                    return obj as T;
                }
                
                T component = obj.GetComponent<T>();

                if (component == null)
                {
                    Debug.LogError("对象不包含组件: " + typeof(T));
                    return null;
                }
                
                return component;
            }
            
            return null;
        }

        public static T SpawnObject<T>(T typePrefab, Vector3 position, Quaternion rotation, PoolType poolType = PoolType.GameObject)
            where T : Component
        {
            return SpawnObject<T>(typePrefab.gameObject, position, rotation, poolType);
        }

        public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation,
            PoolType poolType = PoolType.GameObject)
        {
            return SpawnObject<GameObject>(prefab, position, rotation, poolType);
        }
        
        private static T SpawnObject<T>(GameObject prefab, Transform parent, Quaternion rotation,
            PoolType poolType = PoolType.GameObject) where T : Object
        {
            if (!objectPools.ContainsKey(prefab))
            {
                CreatePool(prefab, parent, rotation, poolType);
            }
            
            GameObject obj = objectPools[prefab].Get();

            if (obj != null)
            {
                if (!cloneToPrefabMap.ContainsKey(obj))
                {
                    cloneToPrefabMap.Add(obj, prefab);
                }
                
                obj.transform.SetParent(parent);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = rotation;
                obj.SetActive(true);

                if (typeof(T) == typeof(GameObject))
                {
                    return obj as T;
                }
                
                T component = obj.GetComponent<T>();

                if (component == null)
                {
                    Debug.LogError("对象不包含组件: " + typeof(T));
                    return null;
                }
                
                return component;
            }
            
            return null;
        }
        
        public static T SpawnObject<T>(T typePrefab, Transform parent, Quaternion rotation, 
            PoolType poolType = PoolType.GameObject)
            where T : Component
        {
            return SpawnObject<T>(typePrefab.gameObject, parent, rotation, poolType);
        }

        public static GameObject SpawnObject(GameObject prefab, Transform parent, Quaternion rotation,
            PoolType poolType = PoolType.GameObject)
        {
            return SpawnObject<GameObject>(prefab, parent, rotation, poolType);
        }

        public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObject)
        {
            if (cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
            {
                GameObject parent = SetParent(poolType);

                if (obj.transform.parent != parent.transform)
                {
                    obj.transform.SetParent(parent.transform);
                }

                if (objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
                {
                    pool.Release(obj);
                }
            }
            else
            {
                Debug.LogWarning("试图返回一个未池化的对象: " + obj.name);
            }
        }

    }

}
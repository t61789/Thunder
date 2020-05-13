using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Tool.ObjectPool
{
    public class ObjectPool : MonoBehaviour
    {
        private const char DIVIDER = '\\';

        private class Pool
        {
            public Queue<IObjectPool> objectQueue = new Queue<IObjectPool>();
            public GameObject prefab;

            public Pool(GameObject prefab)
            {
                objectQueue = new Queue<IObjectPool>();
                this.prefab = prefab;
            }

            public void DestroyOne()
            {
                if (objectQueue.Count != 0)
                {
                    IObjectPool aop = objectQueue.Dequeue();
                    aop.ObjectPoolDestroy();
                }
            }

            public bool QueueIsEmpty()
            {
                return objectQueue.Count == 0;
            }

            public void Enqueue(IObjectPool obj)
            {
                objectQueue.Enqueue(obj);
            }

            public IObjectPool Dequeue()
            {
                return objectQueue.Dequeue();
            }
        }

        public string DefaultPrefabBundle;
        public float ClearTime;

        private float clearTimeStart;
        private readonly Dictionary<string, Pool> objectQueueMap = new Dictionary<string, Pool>();
        private readonly Dictionary<string, GameObject> prefabBuff = new Dictionary<string, GameObject>();
        private readonly StringBuilder pathConverter = new StringBuilder();

        private GameObject GetAsset(string bundlePath,string objName)
        {
            GameObject go = PublicVar.bundleManager.GetAsset<GameObject>(bundlePath, objName);
            if (go == null)
            {
                Debug.LogError(objName + " is not in prefabs");
                return null;
            }
            IObjectPool op = go.GetComponent<IObjectPool>();
            if (op == null)
            {
                Debug.LogError(objName + " is not objectPool");
                Destroy(go);
                return null;
            }
            return go;
        }

        public T DefaultAlloc<T>(string prefabName, Action<T> init = null, Transform container = null) where T : MonoBehaviour
        {
            return Alloc(DefaultPrefabBundle,prefabName,init,container);
        }

        public T Alloc<T>(string prefabPath, Action<T> init = null, Transform container = null) where T : MonoBehaviour
        {
            int split = prefabPath.LastIndexOf('\\');

            return Alloc(prefabPath.Substring(0, split), prefabPath.Substring(split + 1, prefabPath.Length), init, container);
        }

        public T Alloc<T>(string bundlePath,string prefabName, Action<T> init=null, Transform container = null) where T:MonoBehaviour
        {
            string prefabPath = ConvertPath(bundlePath,prefabName);

            if (!objectQueueMap.TryGetValue(prefabPath, out Pool pool))
            {
                GameObject go = GetAsset(bundlePath, prefabName);
                if (!go) return null;
                pool = new Pool(go);
                objectQueueMap.Add(prefabPath, pool);
            }

            IObjectPool aop;
            if (pool.QueueIsEmpty())
            {
                GameObject go = Instantiate(pool.prefab);
                go.name = prefabPath;
                if (container == null)
                    go.transform.SetParent(PublicVar.container);
                else
                    go.transform.SetParent(container);
                aop = go.GetComponent<IObjectPool>();
            }
            else
                aop = pool.Dequeue();

            aop.GetGameObject().SetActive(true);
            aop.ObjectPoolReset();

            T result = aop as T;
            init?.Invoke(result);

            return result;
        }

        protected virtual void Update()
        {
            if (Time.time - clearTimeStart >= ClearTime)
            {
                foreach (var item in objectQueueMap.Values)
                {
                    item.DestroyOne();
                }
                clearTimeStart = Time.time;
            }
        }

        public void Recycle(IObjectPool obj)
        {
            obj.ObjectPoolRecycle();
            obj.GetGameObject().SetActive(false);
            obj.GetGameObject().transform.SetParent(PublicVar.container);
            objectQueueMap[obj.GetGameObject().name].Enqueue(obj);
        }

        public GameObject GetPrefab(string prefabName)
        {
            return GetPrefab(DefaultPrefabBundle,prefabName);
        }

        public GameObject GetPrefab(string bundle, string prefabName)
        {
            string prefabPath = ConvertPath(bundle,prefabName);

            if (!prefabBuff.TryGetValue(prefabPath, out GameObject go))
            {
                go = PublicVar.bundleManager.GetAsset<GameObject>(bundle, prefabName);
                prefabBuff.Add(prefabPath, go);
            }
            return go;
        }

        public string ConvertPath(string bundle, string prefabName)
        {
            pathConverter.Clear();
            pathConverter.Append(bundle);
            pathConverter.Append(DIVIDER);
            pathConverter.Append(prefabName);
            return pathConverter.ToString();
        }
    }
}

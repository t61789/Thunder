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

        public float ClearTime = 5;

        private float clearTimeStart;
        private readonly Dictionary<string, Pool> objectQueueMap = new Dictionary<string, Pool>();
        private readonly Dictionary<string, GameObject> prefabBuff = new Dictionary<string, GameObject>();
        private readonly StringBuilder pathConverter = new StringBuilder();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundlePath">不包含基础路径</param>
        /// <param name="objName"></param>
        /// <returns></returns>
        private GameObject GetAsset(string bundlePath,string objName)
        {
            GameObject go = PublicVar.bundle.GetAsset<GameObject>(BundleManager.PrefabBundleD+ bundlePath, objName);
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefabPath">不包含normal</param>
        /// <param name="init"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public T Alloc<T>(string prefabPath, Action<T> init = null, Transform container = null) where T : MonoBehaviour
        {
            string prefabName;
            string bundlePath;

            int index = prefabPath.LastIndexOf(BundleManager.PathDivider);
            if (index == -1)
            {
                prefabName = prefabPath;
                bundlePath = BundleManager.Normal;
                prefabPath = bundlePath+ BundleManager.PathDivider + prefabPath;
            }
            else
            {
                prefabName = prefabPath.Substring(index + 1);
                bundlePath = prefabPath.Substring(0, index);
            }

            return Alloc(bundlePath, prefabName, prefabPath, init, container);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefabPath">包含normal</param>
        /// <param name="init"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public T Alloc<T>(string bundlePath, string prefabName, Action<T> init=null, Transform container = null) where T : MonoBehaviour
        {
            return Alloc(bundlePath,prefabName,bundlePath+BundleManager.PathDivider+prefabName,init,container);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundlePath">包含normal</param>
        /// <param name="prefabName"></param>
        /// <param name="init"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        private T Alloc<T>(string bundlePath, string prefabName,string prefabPath, Action<T> init, Transform container) where T:MonoBehaviour
        {
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
            {
                aop = pool.Dequeue();
                aop.GetGameObject().transform.SetParent(container);
            }

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

        public GameObject GetPrefab(string prefabPath)
        {
            if (prefabBuff.TryGetValue(prefabPath, out GameObject go))
                return go;

            int index = prefabPath.LastIndexOf('\\');
            string result = index == -1 ? BundleManager.Normal : prefabPath.Substring(0, index);
            string result2 = index == -1 ? prefabPath : prefabPath.Substring(index + 1);

            go = PublicVar.bundle.GetAsset<GameObject>(BundleManager.PrefabBundleD+result, result2);
            prefabBuff.Add(prefabPath, go);

            return go;
        }

        public GameObject GetPrefab(string bundlePath,string prefabName)
        {
            string prefabPath = bundlePath + BundleManager.PathDivider + prefabName;
            if (prefabBuff.TryGetValue(prefabPath, out GameObject go))
                return go;

            go = PublicVar.bundle.GetAsset<GameObject>(bundlePath, prefabName);
            prefabBuff.Add(prefabPath, go);

            return go;
        }

        public string ConvertPath(string bundle, string prefabName)
        {
            pathConverter.Clear();
            pathConverter.Append(bundle);
            pathConverter.Append(BundleManager.PathDivider);
            pathConverter.Append(prefabName);
            return pathConverter.ToString();
        }
    }
}

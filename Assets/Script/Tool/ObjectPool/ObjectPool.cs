using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Tool.ObjectPool
{
    public class ObjectPool : MonoBehaviour
    {
        #region OldCode

        //private class Pool
        //{
        //    public Queue<IObjectPool> objectQueue = new Queue<IObjectPool>();
        //    public GameObject prefab;

        //    public Pool(GameObject prefab)
        //    {
        //        objectQueue = new Queue<IObjectPool>();
        //        this.prefab = prefab;
        //    }

        //    public void DestroyOne()
        //    {
        //        if (objectQueue.Count != 0)
        //        {
        //            IObjectPool aop = objectQueue.Dequeue();
        //            aop.AfterOpDestroy();
        //        }
        //    }

        //    public bool QueueIsEmpty()
        //    {
        //        return objectQueue.Count == 0;
        //    }

        //    public void Enqueue(IObjectPool obj)
        //    {
        //        objectQueue.Enqueue(obj);
        //    }

        //    public IObjectPool Dequeue()
        //    {
        //        return objectQueue.Dequeue();
        //    }
        //}

        //public float ClearTime = 5;

        //private float _ClearTimeStart;
        //private readonly Dictionary<string, Pool> objectQueueMap = new Dictionary<string, Pool>();
        //private readonly Dictionary<string, GameObject> prefabBuff = new Dictionary<string, GameObject>();
        //private readonly StringBuilder pathConverter = new StringBuilder();

        //private GameObject GetAsset(string bundlePath, string objName)
        //{
        //    GameObject go = PublicVar.bundle.GetAsset<GameObject>(BundleManager.PrefabBundleD + bundlePath, objName);
        //    if (go == null)
        //    {
        //        Debug.LogError(objName + " is not in prefabs");
        //        return null;
        //    }
        //    IObjectPool op = go.GetComponent<IObjectPool>();
        //    if (op == null)
        //    {
        //        Debug.LogError(objName + " is not objectPool");
        //        Destroy(go);
        //        return null;
        //    }
        //    return go;
        //}

        //public T Alloc<T>(string prefabPath, Action<T> init = null, Transform container = null) where T : MonoBehaviour
        //{
        //    string prefabName;
        //    string bundlePath;

        //    int index = prefabPath.LastIndexOf(Paths.Div);
        //    if (index == -1)
        //    {
        //        prefabName = prefabPath;
        //        bundlePath = BundleManager.Normal;
        //        prefabPath = bundlePath + Paths.Div + prefabPath;
        //    }
        //    else
        //    {
        //        prefabName = prefabPath.Substring(index + 1);
        //        bundlePath = prefabPath.Substring(0, index);
        //    }

        //    return Alloc(bundlePath, prefabName, prefabPath, init, container);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="prefabPath">包含normal</param>
        ///// <param name="init"></param>
        ///// <param name="container"></param>
        ///// <returns></returns>
        //public T Alloc<T>(string bundlePath, string prefabName, Action<T> init = null, Transform container = null) where T : MonoBehaviour
        //{
        //    return Alloc(bundlePath, prefabName, bundlePath + Paths.Div + prefabName, init, container);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="bundlePath">包含normal</param>
        ///// <param name="prefabName"></param>
        ///// <param name="init"></param>
        ///// <param name="container"></param>
        ///// <returns></returns>
        //private T Alloc<T>(string bundlePath, string prefabName, string prefabPath, Action<T> init, Transform container) where T : MonoBehaviour
        //{
        //    if (!objectQueueMap.TryGetValue(prefabPath, out Pool pool))
        //    {
        //        GameObject go = GetAsset(bundlePath, prefabName);
        //        if (!go) return null;
        //        pool = new Pool(go);
        //        objectQueueMap.Add(prefabPath, pool);
        //    }

        //    IObjectPool aop;
        //    if (pool.QueueIsEmpty())
        //    {
        //        GameObject go = Instantiate(pool.prefab);
        //        go.name = prefabPath;
        //        if (container == null)
        //            go.transform.SetParent(PublicVar.container);
        //        else
        //            go.transform.SetParent(container);
        //        aop = go.GetComponent<IObjectPool>();
        //    }
        //    else
        //    {
        //        aop = pool.Dequeue();
        //        aop.GetGameObject().transform.SetParent(container);
        //    }

        //    aop.GetGameObject().SetActive(true);
        //    aop.BeforeOpReset();

        //    T result = aop as T;
        //    init?.Invoke(result);

        //    return result;
        //}

        //protected virtual void Update()
        //{
        //    if (Time.time - _ClearTimeStart >= ClearTime)
        //    {
        //        foreach (var item in objectQueueMap.Values)
        //        {
        //            item.DestroyOne();
        //        }
        //        _ClearTimeStart = Time.time;
        //    }
        //}

        //public void Recycle(IObjectPool obj)
        //{
        //    obj.BeforeOpRecycle();
        //    obj.GetGameObject().SetActive(false);
        //    obj.GetGameObject().transform.SetParent(PublicVar.container);
        //    objectQueueMap[obj.GetGameObject().name].Enqueue(obj);
        //}

        //public GameObject GetPrefab(string prefabPath)
        //{
        //    if (prefabBuff.TryGetValue(prefabPath, out GameObject go))
        //        return go;

        //    int index = prefabPath.LastIndexOf(Paths.Div);
        //    string result = index == -1 ? BundleManager.Normal : prefabPath.Substring(0, index);
        //    string result2 = index == -1 ? prefabPath : prefabPath.Substring(index + 1);

        //    go = PublicVar.bundle.GetAsset<GameObject>(BundleManager.PrefabBundleD + result, result2);
        //    prefabBuff.Add(prefabPath, go);

        //    return go;
        //}

        //public GameObject GetPrefab(string bundlePath, string prefabName)
        //{
        //    string prefabPath = bundlePath + Paths.Div + prefabName;
        //    if (prefabBuff.TryGetValue(prefabPath, out GameObject go))
        //        return go;

        //    go = PublicVar.bundle.GetAsset<GameObject>(bundlePath, prefabName);
        //    prefabBuff.Add(prefabPath, go);

        //    return go;
        //}

        //public string ConvertPath(string bundle, string prefabName)
        //{
        //    pathConverter.Clear();
        //    pathConverter.Append(bundle);
        //    pathConverter.Append(Paths.Div);
        //    pathConverter.Append(prefabName);
        //    return pathConverter.ToString();
        //}
        #endregion

        private class Pool
        {
            private readonly Queue<IObjectPool> _ObjectQueue;
            public readonly GameObject Prefab;
            public readonly bool Allocable;

            public Pool(GameObject prefab,bool allocable)
            {
                _ObjectQueue = new Queue<IObjectPool>();
                Prefab = prefab;
                Allocable = allocable;
            }

            public void DestroyOne()
            {
                if (_ObjectQueue.Count == 0) return;
                IObjectPool aop = _ObjectQueue.Dequeue();
                aop.AfterOpDestroy();
            }

            public bool QueueIsEmpty()
            {
                return _ObjectQueue.Count == 0;
            }

            public void Enqueue(IObjectPool obj)
            {
                _ObjectQueue.Enqueue(obj);
            }

            public IObjectPool Dequeue()
            {
                return _ObjectQueue.Dequeue();
            }
        }

        public float ClearTime = 5;
        private float _ClearTimeStart;

        private readonly Dictionary<AssetId, Pool> _Pools = new Dictionary<AssetId, Pool>();

        private void CreatePools(string bundleGroup, string bundle)
        {
            AssetId id = new AssetId(bundleGroup, bundle, null);
            foreach (var prefab in PublicVar.bundle.GetAllAsset<GameObject>(bundleGroup, bundle))
            {
                id.Name = prefab.name;
                if (_Pools.ContainsKey(id)) continue;
                _Pools.Add(id, new Pool(prefab, prefab.GetComponent<IObjectPool>() != null));
            }
            PublicVar.bundle.ReleaseBundle(bundleGroup, bundle);
        }

        public T Alloc<T>(string prefabPath, Action<T> init = null, Transform container = null) where T : MonoBehaviour
        {
            var split = prefabPath?.Split('!');
            // ReSharper disable once PossibleNullReferenceException
            Assert.IsFalse(split == null && split.Length < 1 && split.Length > 3,
                $"prefab路径不正确：{prefabPath}");
            switch (split.Length)
            {
                case 1:
                    return Alloc(null, null, split[0], init, container);
                case 2:
                    return Alloc(null, split[0], split[1], init, container);
                case 3:
                    return Alloc(split[0], split[1], split[2], init, container);
                default:
                    return null;
            }
        }

        public T Alloc<T>(string bundleGroup, string bundle, string name, Action<T> init = null, Transform container = null) where T : MonoBehaviour
        {
            Pool pool = GetPool(bundleGroup, bundle, name,out AssetId assetId);
            Assert.IsTrue(pool.Allocable,$"{assetId.BundleGroup}!{assetId.Bundle}!{assetId.Name} 没有挂载实现IObjectPool的脚本，不可alloc");

            IObjectPool aop;
            // ReSharper disable once PossibleNullReferenceException
            if (pool.QueueIsEmpty())
            {
                GameObject go = Instantiate(pool.Prefab);
                go.transform.SetParent(container ?? PublicVar.container);
                aop = go.GetComponent<IObjectPool>();
                aop.AssetId = assetId;
            }
            else
            {
                aop = pool.Dequeue();
                (aop as MonoBehaviour)?.transform.SetParent(container);
            }

            (aop as MonoBehaviour)?.gameObject.SetActive(true);
            aop.BeforeOpReset();

            T result = aop as T;
            init?.Invoke(result);

            return result;
        }

        private Pool GetPool(string bundleGroup, string bundle, string name,out AssetId id)
        {
            id = CreateId(bundleGroup, bundle, name);

            if (_Pools.TryGetValue(id, out Pool pool)) return pool;
            CreatePools(id.BundleGroup, id.Bundle);
            Assert.IsTrue(_Pools.TryGetValue(id, out pool), $"未找到prefab {bundleGroup}!{bundle}!{name}");

            return pool;
        }

        protected void Update()
        {
            if (!(Time.time - _ClearTimeStart >= ClearTime)) return;
            foreach (var item in _Pools.Values)
                item.DestroyOne();
            _ClearTimeStart = Time.time;
        }

        public void Recycle(IObjectPool obj)
        {
            obj.BeforeOpRecycle();
            GameObject mono = (obj as MonoBehaviour)?.gameObject;
            mono?.SetActive(false);
            mono?.transform.SetParent(PublicVar.container);
            _Pools[obj.AssetId].Enqueue(obj);
        }

        public GameObject GetPrefab(string prefabPath)
        {
            var split = prefabPath?.Split('!');
            // ReSharper disable once PossibleNullReferenceException
            Assert.IsFalse(split == null && split.Length < 1 && split.Length > 3,
                $"prefab路径不正确：{prefabPath}");
            switch (split.Length)
            {
                case 1:
                    return GetPrefab(null, null, split[0]);
                case 2:
                    return GetPrefab(null, split[0], split[1]);
                case 3:
                    return GetPrefab(split[0], split[1], split[2]);
                default:
                    return null;
            }
        }
        public GameObject GetPrefab(string bundleGroup, string bundle, string name)
        {
            return GetPool(bundleGroup, bundle, name,out _).Prefab;
        }

        private static readonly string DefaultBundle = BundleManager.PrefabBundleD + BundleManager.Normal;
        private static AssetId CreateId(string bundleGroup, string bundle, string name)
        {
            bundle = bundle ?? DefaultBundle;
            return new AssetId(bundleGroup, bundle, name);
        }
    }

    public interface IObjectPool
    {
        void BeforeOpReset();
        void BeforeOpRecycle();
        void AfterOpDestroy();
        AssetId AssetId { get; set; }
    }
}

using System.Collections.Generic;
using Thunder.Utility;
using Tool;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tool
{
    public class ObjectPool : MonoBehaviour, IBaseSys
    {
        public float ClearTime = 5;

        private readonly Dictionary<AssetId, Pool> _Pools = new Dictionary<AssetId, Pool>();

        private static readonly string _DefaultBundle = Paths.PrefabBundleD + Paths.Normal;

        public static ObjectPool Ins { get; private set; }
        
        private void Awake()
        {
            Ins = this;
            new AutoCounter(this, ClearTime).OnComplete(() =>
            {
                foreach (var item in _Pools.Values)
                    item.DestroyOne();
            });
            SceneManager.sceneUnloaded += x => _Pools.Clear();
        }

        public T Alloc<T>(string assetPath) where T : MonoBehaviour
        {
            return Alloc<T>(AssetId.Parse(assetPath,_DefaultBundle));
        }

        public T Alloc<T>(AssetId assetId) where T : MonoBehaviour
        {
            var pool = GetPool(assetId);
            if (!pool.Allocable)
                return default;

            IObjectPool aop;
            if (pool.QueueIsEmpty())
            {
                var go = Instantiate(pool.Prefab);
                go.transform.SetParent(GameCore.Container);
                aop = go.GetComponent<IObjectPool>();
                aop.AssetId = assetId;
            }
            else
            {
                aop = pool.Dequeue();
                (aop as MonoBehaviour).transform.SetParent(GameCore.Container);
            }

            (aop as MonoBehaviour).gameObject.SetActive(true);
            aop.OpReset();

            return aop as T;
        }

        public void Recycle(IObjectPool obj)
        {
            obj.OpRecycle();
            var gameObj = (obj as MonoBehaviour).gameObject;
            gameObj.SetActive(false);
            gameObj.transform.SetParent(GameCore.Container);
            _Pools[obj.AssetId].Enqueue(obj);
        }

        public GameObject GetPrefab(string assetPath)
        {
            return GetPool(AssetId.Parse(assetPath,_DefaultBundle)).Prefab;
        }

        private Pool GetPool(AssetId id)
        {
            if (_Pools.TryGetValue(id, out var pool)) return pool;
            CreatePool(id);
            if (!_Pools.TryGetValue(id, out pool))
                throw new ResourceNotFoundException($"未找到prefab {id}");

            return pool;
        }

        private void CreatePool(AssetId id)
        {
            var prefab = BundleSys.Ins.GetAsset<GameObject>(id);
            _Pools.Add(id, new Pool(prefab, prefab.GetComponent<IObjectPool>() != null));
        }

        public void OnSceneEnter(string preScene, string curScene)
        {
        }

        public void OnSceneExit(string curScene)
        {
        }

        public void OnApplicationExit()
        {
        }
        
        private class Pool
        {
            private readonly Queue<IObjectPool> _ObjectQueue;
            public readonly bool Allocable;
            public readonly GameObject Prefab;

            public Pool(GameObject prefab, bool allocable)
            {
                _ObjectQueue = new Queue<IObjectPool>();
                Prefab = prefab;
                Allocable = allocable;
            }

            public void DestroyOne()
            {
                if (_ObjectQueue.Count == 0) return;
                var aop = _ObjectQueue.Dequeue();
                aop.OpDestroy();
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
    }

    public interface IObjectPool
    {
        AssetId AssetId { get; set; }
        void OpReset();
        void OpRecycle();
        void OpDestroy();
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
    /// <summary>
    /// 对象池。可调用的游戏物体上必须附加有至少一个继承了IObjectPool的组件
    /// <br/>使用Get请求一个对象，转型他的组件后返回
    /// <br/>使用Put回收对象，将其放入对象池中
    /// </summary>
    public class GameObjectPool : MonoBehaviour, IBaseSys
    {
        public float ClearTime = 5;

        private static readonly Dictionary<AssetId, Pool> _Pools = new Dictionary<AssetId, Pool>();
        private static GameObjectPool _Ins;

        private void Awake()
        {
            if(_Ins!=null)
                throw new InitDuplicatelyException();

            _Ins = this;
            new AutoCounter(this, ClearTime).OnComplete(() =>
            {
                foreach (var item in _Pools.Values)
                    item.DestroyOne();
            });
            SceneManager.sceneUnloaded += x => _Pools.Clear();
        }

        /// <summary>
        /// 从对象池中取出对象
        /// </summary>
        /// <typeparam name="T">需要转换的类型</typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static T Get<T>(string assetPath) where T : MonoBehaviour
        {
            return Get<T>(AssetId.Parse(assetPath));
        }

        /// <summary>
        /// 从对象池中取出对象
        /// </summary>
        /// <typeparam name="T">需要转换的类型</typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public static T Get<T>(AssetId assetId) where T : MonoBehaviour
        {
            if(_Ins==null)
                throw new Exception("对象池未初始化");

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

        /// <summary>
        /// 将对象放入对象池
        /// </summary>
        /// <param name="obj"></param>
        public static void Put(IObjectPool obj)
        {
            obj.OpPut();
            var gameObj = (obj as MonoBehaviour).gameObject;
            gameObj.SetActive(false);
            gameObj.transform.SetParent(GameCore.Container);
            _Pools[obj.AssetId].Enqueue(obj);
        }

        public static GameObject GetPrefab(string assetPath)
        {
            return GetPool(AssetId.Parse(assetPath)).Prefab;
        }

        private static Pool GetPool(AssetId id)
        {
            if (_Pools.TryGetValue(id, out var pool)) return pool;
            CreatePool(id);
            if (!_Pools.TryGetValue(id, out pool))
                throw new ResourceNotFoundException($"未找到prefab {id}");

            return pool;
        }

        private static void CreatePool(AssetId id)
        {
            var prefab = BundleSys.GetAsset<GameObject>(id);
            _Pools.Add(id, new Pool(prefab, prefab.GetComponent<IObjectPool>() != null));
        }

        public void OnSceneEnter(string preScene, string curScene){}

        public void OnSceneExit(string curScene){}

        public void OnApplicationExit(){}
        
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
        /// <summary>
        /// 游戏物体的资源ID
        /// </summary>
        AssetId AssetId { get; set; }
        /// <summary>
        /// 从对象池中取出时调用
        /// </summary>
        void OpReset();
        /// <summary>
        /// 放入对象池时调用
        /// </summary>
        void OpPut();
        /// <summary>
        /// 在池中被销毁时调用
        /// </summary>
        void OpDestroy();
    }
}
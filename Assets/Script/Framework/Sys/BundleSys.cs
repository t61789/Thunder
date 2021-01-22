using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    /// <summary>
    /// 管理bundle资源的加载与卸载
    /// </summary>
    public class BundleSys : IBaseSys
    {
        private static Dictionary<string, BundleUnit> _Bundles;
        private static AssetBundle _ManifestBundle;
        private static AssetBundleManifest _Manifest;
        private static readonly Queue<string> _LoadQueue = new Queue<string>();    // 用于加载bundle的队列

        public BundleSys()
        {
            // 读取Mainfest文件，用于获取依赖信息
            _ManifestBundle = AssetBundle.LoadFromFile(Paths.BundleBasePath.PCombine(Path.GetFileName(Paths.BundleBasePath)));
            _Manifest = _ManifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            _Bundles = new Dictionary<string, BundleUnit>();
        }

        public static T GetAsset<T>(string assetPath) where T : Object
        {
            return GetAsset<T>(AssetId.Parse(assetPath));
        }

        public static T GetAsset<T>(AssetId assetId) where T : Object
        {
            var result = GetBundle(assetId.Bundle).Bundle.LoadAsset<T>(assetId.Name);
            if (result == null)
                throw new Exception($"未在bundle {assetId.Bundle} 中找到名为 {assetId.Name} 的asset");
            return result;
        }

        public static T[] GetAllAsset<T>(string bundle) where T : Object
        {
            return GetBundle(bundle).Bundle.LoadAllAssets<T>();
        }

        public static void LoadBundle(string bundle)
        {
            GetBundle(bundle);
        }

        public static void ReleaseBundle(string bundle)
        {
            _LoadQueue.Clear();
            _LoadQueue.Enqueue(bundle);
            var first = true;

            while (_LoadQueue.Count != 0)
            {
                var curBundle = _LoadQueue.Dequeue();
                var b = _Bundles[curBundle];
                if (!first && b.DependencyByCount == 0)
                    throw new BundleDependencyException(curBundle, b.DependencyByCount);

                b.DependencyByCount--;
                if (b.DependencyByCount < 1)
                {
                    foreach (var item in b.Dependencies)
                        _LoadQueue.Enqueue(item);
                    b.Bundle.Unload(false);
                    _Bundles.Remove(curBundle);
                }

                first = false;
            }
        }

        public static void ReleaseAllBundles()
        {
            while (_Bundles.Count != 0)
                ReleaseBundle(_Bundles.Keys.First());
        }

        public static void Clear()
        {
            ReleaseAllBundles();
            _Manifest = null;
            _ManifestBundle.Unload(true);
        }

        private static BundleUnit GetBundle(string bundle)
        {
            // 非递归地加载所有依赖的bundle
            _LoadQueue.Clear();
            _LoadQueue.Enqueue(bundle);
            var first = true;

            while (_LoadQueue.Count != 0)
            {
                var curBundle = _LoadQueue.Dequeue();

                if (_Bundles.TryGetValue(curBundle, out var temp))
                {
                    if (!first)
                        temp.DependencyByCount++;
                    else
                        break;
                }
                else
                {
                    var assetBundle = AssetBundle.LoadFromFile(Path.Combine(Paths.BundleBasePath, curBundle));
                    var newBundle = new BundleUnit(assetBundle, !first);
                    first = false;
                    _Bundles.Add(curBundle, newBundle);

                    foreach (var item in _Manifest.GetAllDependencies(curBundle))
                    {
                        _LoadQueue.Enqueue(item);
                        newBundle.Dependencies.Add(item);
                    }
                }
            }

            return _Bundles[bundle];
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

        private class BundleUnit
        {
            public readonly AssetBundle Bundle;
            public readonly List<string> Dependencies;
            public int DependencyByCount;

            public BundleUnit(AssetBundle bundle, bool dep)
            {
                Bundle = bundle;
                Dependencies = new List<string>();
                DependencyByCount = 0;
                if (dep)
                    DependencyByCount++;
            }
        }
    }

    /// <summary>
    /// 唯一地定位一个资源的ID
    /// </summary>
    public struct AssetId
    {
        public readonly string Bundle;
        public string Name;

        // 缓存转换过的字符串
        private static readonly LRUCache<string, AssetId> _Cache =
            new LRUCache<string, AssetId>(40);

        private static readonly StringBuilder _StringBuilder =
            new StringBuilder();

        public AssetId(string bundle, string name)
        {
            Bundle = bundle;
            Name = name;
        }

        /// <summary>
        /// 格式 [bundle/]asset
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static AssetId Parse(string assetPath)
        {
            if (_Cache.Contains(assetPath)) return _Cache.Get(assetPath);

            if (string.IsNullOrEmpty(assetPath))
                throw AssetPathInvalidException.Default;

            var lastDiv = assetPath.LastIndexOf('/');
            if(lastDiv==-1)
                throw AssetPathInvalidException.Default;

            var result = new AssetId(
                assetPath.Substring(0,lastDiv),
                assetPath.Substring(lastDiv+1));

            _Cache.Add(assetPath, result);
            return result;
        }

        public static string Combine(string bundle, string name)
        {
            _StringBuilder.Clear();
            _StringBuilder.Append(bundle);
            _StringBuilder.Append('/');
            _StringBuilder.Append(name);
            return _StringBuilder.ToString();
        }

        public override string ToString()
        {
            return Combine(Bundle, Name);
        }
    }
}
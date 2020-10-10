using System;
using System.Collections.Generic;
using System.IO;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Thunder.Sys
{
    public class BundleSys : IBaseSys
    {
        private readonly Dictionary<string, BundleGroup> _BundleGroups = new Dictionary<string, BundleGroup>();

        private readonly Queue<string> _LoadQueue = new Queue<string>();

        public BundleSys()
        {
            Ins = this;
            LoadBundleGroup(Paths.BundleBasePath);
        }

        public static BundleSys Ins { get; private set; }

        public void OnSceneEnter(string preScene, string curScene)
        {
        }

        public void OnSceneExit(string curScene)
        {
        }

        public void OnApplicationExit()
        {
        }

        public void LoadBundleGroup(string bundleGroupDir)
        {
            bundleGroupDir = Path.GetFullPath(bundleGroupDir);
            _BundleGroups.Add(bundleGroupDir, new BundleGroup(bundleGroupDir));
        }

        public void ReleaseBundleGroup(string bundleGroupDir)
        {
            BundleGroupCheck(ref bundleGroupDir).Release();
            _BundleGroups.Remove(bundleGroupDir);
        }

        public void ReleaseAllBundleGroup()
        {
            foreach (var bundleGroup in _BundleGroups.Values)
                bundleGroup.Release();
            _BundleGroups.Clear();
        }

        private BundleUnit GetBundle(string bundleGroup, string bundle)
        {
            // 非递归地加载所有依赖的bundle

            var groupUnit = BundleGroupCheck(ref bundleGroup);

            _LoadQueue.Clear();
            _LoadQueue.Enqueue(bundle);
            var first = true;

            while (_LoadQueue.Count != 0)
            {
                var curBundle = _LoadQueue.Dequeue();

                if (groupUnit.Bundles.TryGetValue(curBundle, out var temp) && !first)
                {
                    temp.DependencyByCount++;
                }
                else
                {
                    var assetBundle = AssetBundle.LoadFromFile(Path.Combine(bundleGroup, curBundle));
                    var newBundle = new BundleUnit(assetBundle, !first);
                    first = false;
                    groupUnit.Bundles.Add(curBundle, newBundle);

                    foreach (var item in groupUnit.Mainfest.GetAllDependencies(curBundle))
                    {
                        _LoadQueue.Enqueue(item);
                        newBundle.Dependencies.Add(item);
                    }
                }
            }

            return groupUnit.Bundles[bundle];
        }

        public void ReleaseBundle(string bundleGroup, string bundle)
        {
            var groupUnit = BundleGroupCheck(ref bundleGroup);
            Assert.IsTrue(groupUnit.Bundles.TryGetValue(bundle, out _), bundleGroup + " 中不存在名为 " + bundle + " 的bundle");

            _LoadQueue.Clear();
            _LoadQueue.Enqueue(bundle);
            var first = true;

            while (_LoadQueue.Count != 0)
            {
                var curBundle = _LoadQueue.Dequeue();
                var b = groupUnit.Bundles[curBundle];
                Assert.IsFalse(first && b.DependencyByCount != 0,
                    curBundle + " 仍有 " + b.DependencyByCount + " 个bundle依赖于它，不能释放");

                b.DependencyByCount--;
                if (b.DependencyByCount < 1)
                {
                    foreach (var item in b.Dependencies)
                        _LoadQueue.Enqueue(item);
                    b.Bundle.Unload(false);
                    groupUnit.Bundles.Remove(curBundle);
                }

                first = false;
            }
        }

        private BundleGroup BundleGroupCheck(ref string bundleGroup)
        {
            bundleGroup = bundleGroup ?? Paths.BundleBasePath;
            bundleGroup = Path.GetFullPath(bundleGroup);
            var temp = _BundleGroups.TryGetValue(bundleGroup, out var groupUnit);
            Assert.IsTrue(temp, "未加载名为 " + bundleGroup + " 的bundleGroup");
            return groupUnit;
        }

        public T GetAsset<T>(string assetPath) where T : Object
        {
            var split = assetPath.LastIndexOf(Paths.Div);

            return GetAsset<T>(Paths.BundleBasePath, assetPath.Substring(0, split),
                assetPath.Substring(split + 1, assetPath.Length));
        }

        public T GetAsset<T>(string bundle, string asset) where T : Object
        {
            return GetAsset<T>(Paths.BundleBasePath, bundle, asset);
        }

        public T GetAsset<T>(string bundleGroup, string bundle, string asset) where T : Object
        {
            var result = GetBundle(bundleGroup, bundle).Bundle.LoadAsset<T>(asset);
            Assert.IsNotNull(result, "未在bundle " + bundle + " 中找到名为 " + asset + " 的asset");
            return result;
        }

        public T[] GetAllAsset<T>(string bundle) where T : Object
        {
            return GetAllAsset<T>(null, bundle);
        }

        public T[] GetAllAsset<T>(string bundleGroup, string bundle) where T : Object
        {
            BundleGroupCheck(ref bundleGroup);

            return GetBundle(bundleGroup, bundle).Bundle.LoadAllAssets<T>();
        }

        public void Reset()
        {
            ReleaseAllBundleGroup();
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

        private class BundleGroup
        {
            // BundleGroup对应了一个bundle的文件夹，在最高层上应当有整个文件夹的mainfest文件
            // mainfest文件也应与文件夹同名

            public readonly Dictionary<string, BundleUnit> Bundles;
            private AssetBundle _MainfestBundle;
            public AssetBundleManifest Mainfest;

            public BundleGroup(string bundleDir)
            {
                _MainfestBundle = AssetBundle.LoadFromFile(bundleDir + Paths.Div + Path.GetFileName(bundleDir));
                Mainfest = _MainfestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

                Bundles = new Dictionary<string, BundleUnit>();
            }

            public void Release()
            {
                foreach (var item in Bundles.Values)
                    item.Bundle.Unload(false);
                _MainfestBundle.Unload(false);
                Mainfest = null;
                _MainfestBundle = null;
            }
        }

        #region oldCode

        //private readonly Dictionary<string, Bundle> bundles = new Dictionary<string, Bundle>();
        //private readonly AssetBundleManifest manifest;
        //private readonly AssetBundle mainfestBundle;

        //public BundleSys()
        //{
        //    mainfestBundle = AssetBundle.LoadFromFile(Paths.BundleBasePathD + Path.GetFileName(Paths.BundleBasePath));
        //    manifest = mainfestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        //}

        //public T GetAsset<T>(string assetPath) where T : Object
        //{
        //    int split = assetPath.LastIndexOf(Paths.Div);

        //    return GetAsset<T>(assetPath.Substring(0, split), assetPath.Substring(split + 1, assetPath.Length));
        //}

        //public T GetAsset<T>(string bundlePath, string asset) where T : Object
        //{
        //    T result = GetBundle(bundlePath).Bundle.LoadAsset<T>(asset);
        //    if (result == null)
        //        Debug.LogError("No asset named " + asset + " in Bundle " + bundlePath);
        //    return result;
        //}

        //public T[] GetAllAsset<T>(string bundlePath) where T : Object
        //{
        //    return GetBundle(bundlePath).Bundle.LoadAllAssets<T>();
        //}


        //private Bundle GetBundle(string bundlePath)
        //{
        //    _LoadQueue.Clear();
        //    _LoadQueue.Enqueue(bundlePath);
        //    bool first = true;

        //    while (_LoadQueue.Count != 0)
        //    {
        //        string curBundle = _LoadQueue.Dequeue();

        //        if (!bundles.TryGetValue(curBundle, out Bundle temp))
        //        {
        //            AssetBundle assetBundle = AssetBundle.LoadFromFile(Paths.BundleBasePath + Paths.Div + curBundle);
        //            Bundle newBundle = new Bundle(assetBundle, !first);
        //            first = false;
        //            bundles.Add(curBundle, newBundle);

        //            foreach (var item in manifest.GetAllDependencies(curBundle))
        //            {
        //                _LoadQueue.Enqueue(item);
        //                newBundle.dependencies.Add(item);
        //            }
        //        }
        //        else
        //        {
        //            if (!first)
        //                temp.dependencyByCount++;
        //            else
        //                return temp;
        //        }
        //    }

        //    return bundles[bundlePath];
        //}

        //public bool ReleaseBundle(string bundlePath)
        //{
        //    _LoadQueue.Clear();
        //    _LoadQueue.Enqueue(bundlePath);
        //    bool first = true;

        //    while (_LoadQueue.Count != 0)
        //    {
        //        string curBundle = _LoadQueue.Dequeue();
        //        if (bundles.TryGetValue(curBundle, out Bundle b))
        //        {
        //            if (!first)
        //            {
        //                b.dependencyByCount--;
        //                if (b.dependencyByCount != 0)
        //                    continue;
        //            }
        //            else
        //            {
        //                if (b.dependencyByCount != 0)
        //                    return false;
        //                first = false;
        //            }

        //            foreach (var item in b.dependencies)
        //                _LoadQueue.Enqueue(item);
        //            b.Bundle.Unload(true);
        //            bundles.Dequeue(curBundle);
        //        }
        //        else
        //            return false;
        //    }

        //    return true;
        //}

        //public void ReleaseAllBundle(bool force)
        //{
        //    foreach (var item in bundles.Values)
        //        item.Bundle.Unload(true);
        //    if (force)
        //        mainfestBundle.Unload(true);

        //    bundles.Clear();
        //}

        #endregion
    }

    public struct AssetId
    {
        public readonly string BundleGroup;
        public readonly string Bundle;
        public string Name;

        private static readonly LRUCache<string, AssetId> _Cache =
            new LRUCache<string, AssetId>(GlobalSettings.AssetIdCacheSize);

        public AssetId(string bundleGroup, string bundle, string name, string defaultBundle = null)
        {
            BundleGroup = Path.GetFullPath(bundleGroup ?? Paths.BundleBasePath);
            Bundle = bundle ?? defaultBundle;
            Name = name;
        }

        /// <summary>
        ///     格式 [bundleGroup!][bundle!]asset
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="defaultBundle"></param>
        /// <returns></returns>
        public static AssetId Create(string assetPath, string defaultBundle = null)
        {
            if (_Cache.Contains(assetPath)) return _Cache.Get(assetPath);
            var split = assetPath.Split('!');
            split = split.Length == 0 ? new string[1] : split;
            Assert.IsTrue(split.Length <= 3, $"路径不正确：{assetPath}");

            var newa = new string[3];
            Array.Copy(split, 0, newa, 3 - split.Length, split.Length);
            var result = new AssetId(newa[0], newa[1], newa[2], defaultBundle);

            _Cache.Put(assetPath, result);
            return result;
        }

        public static bool BundleEquals(AssetId id1, AssetId id2)
        {
            return id1.BundleGroup == id2.BundleGroup && id1.Bundle == id2.Bundle;
        }
    }
}
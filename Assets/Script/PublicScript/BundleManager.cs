using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.AssetBundlePatching;

public class BundleManager
{
    public static readonly string DllBundle = @"dll";
    public static readonly string DllBundleD = @"dll" + Paths.Div;
    public static readonly string PrefabBundle = @"prefabs";
    public static readonly string PrefabBundleD = @"prefabs" + Paths.Div;
    public static readonly string UIBundle = @"ui";
    public static readonly string UIBundleD = @"ui" + Paths.Div;
    public static readonly string ValuesBundle = @"values";
    public static readonly string ValuesBundleD = @"values" + Paths.Div;
    public static readonly string DatabaseBundle = @"database";
    public static readonly string DatabaseBundleD = @"database" + Paths.Div;

    public static readonly string Normal = @"normal";
    public static readonly string NormalD = @"normal" + Paths.Div;

    #region oldCode
    //private readonly Dictionary<string, Bundle> bundles = new Dictionary<string, Bundle>();
    //private readonly AssetBundleManifest manifest;
    //private readonly AssetBundle mainfestBundle;

    //public BundleManager()
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
    //            bundles.Remove(curBundle);
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
        public AssetBundleManifest Mainfest;
        private AssetBundle _MainfestBundle;

        public BundleGroup(string bundleDir)
        {
            _MainfestBundle = AssetBundle.LoadFromFile(bundleDir + Paths.Div + Path.GetFileName(bundleDir));
            Mainfest = _MainfestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            Bundles = new Dictionary<string, BundleUnit>();
        }

        public void Release()
        {
            foreach (var item in Bundles.Values)
                item.Bundle.Unload(true);
            _MainfestBundle.Unload(true);
            Mainfest = null;
            _MainfestBundle = null;
        }
    }

    private readonly Dictionary<string, BundleGroup> _BundleGroups = new Dictionary<string, BundleGroup>();

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

    private readonly Queue<string> _LoadQueue = new Queue<string>();

    private BundleUnit GetBundle(string bundleGroup,string bundle )
    {
        // 非递归地加载所有依赖的bundle

        BundleGroup groupUnit = BundleGroupCheck(ref bundleGroup);

        _LoadQueue.Clear();
        _LoadQueue.Enqueue(bundle);
        bool first = true;

        while (_LoadQueue.Count != 0)
        {
            string curBundle = _LoadQueue.Dequeue();

            if (groupUnit.Bundles.TryGetValue(curBundle, out BundleUnit temp) && !first)
            {
                temp.DependencyByCount++;
            }
            else
            {
                AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(bundleGroup, curBundle));
                BundleUnit newBundle = new BundleUnit(assetBundle, !first);
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
    
    public void ReleaseBundle(string bundleGroup,string bundle)
    {
        BundleGroup groupUnit = BundleGroupCheck(ref bundleGroup);
        Assert.IsTrue(groupUnit.Bundles.TryGetValue(bundle,out _), bundleGroup+" 中不存在名为 " + bundle +" 的bundle");

        _LoadQueue.Clear();
        _LoadQueue.Enqueue(bundle);
        bool first = true;

        while (_LoadQueue.Count != 0)
        {
            string curBundle = _LoadQueue.Dequeue();
            BundleUnit b = groupUnit.Bundles[curBundle];
            Assert.IsFalse(first && b.DependencyByCount==0,curBundle+" 仍有 "+b.DependencyByCount+" 个bundle依赖于它，不能释放");

            b.DependencyByCount--;
            if (b.DependencyByCount < 1)
            {
                foreach (var item in b.Dependencies)
                    _LoadQueue.Enqueue(item);
                b.Bundle.Unload(true);
                groupUnit.Bundles.Remove(curBundle);
            }

            first = false;
        }
    }

    private BundleGroup BundleGroupCheck(ref string bundleGroup)
    {
        bundleGroup ??= Paths.BundleBasePath;
        bundleGroup = Path.GetFullPath(bundleGroup);
        Assert.IsTrue(_BundleGroups.TryGetValue(bundleGroup, out BundleGroup groupUnit), "未加载名为 " + bundleGroup + " 的bundleGroup");
        return groupUnit;
    }

    public T GetAsset<T>(string assetPath) where T : Object
    {
        int split = assetPath.LastIndexOf(Paths.Div);

        return GetAsset<T>(Paths.BundleBasePath, assetPath.Substring(0, split), assetPath.Substring(split + 1, assetPath.Length));
    }

    public T GetAsset<T>(string bundle, string asset) where T : Object
    {
        return GetAsset<T>(Paths.BundleBasePath,bundle,asset);
    }

    public T GetAsset<T>(string bundleGroup, string bundle, string asset) where T : Object
    {
        T result = GetBundle(bundleGroup,bundle).Bundle.LoadAsset<T>(asset);
        Assert.IsNotNull(result,"未在bundle "+bundle+" 中找到名为 "+asset+" 的asset");
        return result;
    }

    public T[] GetAllAsset<T>(string bundle) where T : Object
    {
        return GetAllAsset<T>(null, bundle);
    }

    public T[] GetAllAsset<T>(string bundleGroup, string bundle) where T : Object
    {
        BundleGroupCheck(ref bundleGroup);

        return GetBundle(bundleGroup,bundle).Bundle.LoadAllAssets<T>();
    }
}


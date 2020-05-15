using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class BundleManager
{
    public static readonly string DllBundle = @"dll";
    public static readonly string DllBundleD = @"dll\";
    public static readonly string PrefabBundle = @"prefabs";
    public static readonly string PrefabBundleD = @"prefabs\";
    public static readonly string UIBundle = @"ui";
    public static readonly string UIBundleD = @"ui\";
    public static readonly string ValuesBundle = @"values";
    public static readonly string ValuesBundleD = @"values\";
    public static readonly string DatabaseBundle = @"database";
    public static readonly string DatabaseBundleD = @"database\";

    public static readonly string Normal = @"normal";
    public static readonly string NormalD = @"normal\";
    public static readonly char PathDivider = '\\';

    public static string BundleBasePath = @"E:\AssetBundles\StandaloneWindows";

    private struct Bundle
    {
        public AssetBundle bundle;
        public List<string> dependencies;
        public int dependencyByCount;

        public Bundle(AssetBundle bundle, bool dep)
        {
            this.bundle = bundle;
            dependencies = new List<string>();
            dependencyByCount = 0;
            if (dep)
                dependencyByCount++;
        }
    }

    private readonly Dictionary<string, Bundle> bundles = new Dictionary<string, Bundle>();
    private readonly AssetBundleManifest manifest;
    private readonly AssetBundle mainfestBundle;

    public BundleManager()
    {
        mainfestBundle = AssetBundle.LoadFromFile(BundleBasePath + "\\" + Path.GetFileName(BundleBasePath));
        manifest = mainfestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }

    public T GetAsset<T>(string assetPath) where T : Object
    {
        int split = assetPath.LastIndexOf('\\');

        return GetAsset<T>(assetPath.Substring(0,split), assetPath.Substring(split+1, assetPath.Length));
    }

    public T GetAsset<T>(string bundlePath, string asset) where T : Object
    {
        T result = GetBundle(bundlePath).bundle.LoadAsset<T>(asset);
        if (result == null)
            Debug.LogError("No asset named " + asset + " in bundle " + bundlePath);
        return result;
    }

    public T[] GetAllAsset<T>(string bundlePath) where T : Object
    {
        return GetBundle(bundlePath).bundle.LoadAllAssets<T>();
    }

    private readonly Queue<string> loadQueue = new Queue<string>();

    private Bundle GetBundle(string bundlePath)
    {
        loadQueue.Clear();
        loadQueue.Enqueue(bundlePath);
        bool first = true;

        while (loadQueue.Count != 0)
        {
            string curBundle = loadQueue.Dequeue();

            if (!bundles.TryGetValue(curBundle, out Bundle temp))
            {
                AssetBundle assetBundle = AssetBundle.LoadFromFile(BundleBasePath + PathDivider + curBundle);
                Bundle newBundle = new Bundle(assetBundle, !first);
                first = false;
                bundles.Add(curBundle, newBundle);

                foreach (var item in manifest.GetAllDependencies(curBundle))
                {
                    loadQueue.Enqueue(item);
                    newBundle.dependencies.Add(item);
                }
            }
            else
            {
                if (!first)
                    temp.dependencyByCount++;
                else
                    return temp;
            }
        }

        return bundles[bundlePath];
    }

    public bool ReleaseBundle(string bundlePath)
    {
        loadQueue.Clear();
        loadQueue.Enqueue(bundlePath);
        bool first = true;

        while (loadQueue.Count != 0)
        {
            string curBundle = loadQueue.Dequeue();
            if (bundles.TryGetValue(curBundle, out Bundle b))
            {
                if (!first)
                {
                    b.dependencyByCount--;
                    if (b.dependencyByCount != 0)
                        continue;
                }
                else
                {
                    if (b.dependencyByCount != 0)
                        return false;
                    first = false;
                }

                foreach (var item in b.dependencies)
                    loadQueue.Enqueue(item);
                b.bundle.Unload(true);
                bundles.Remove(curBundle);
            }
            else
                return false;
        }

        return true;
    }

    public void ReleaseAllBundle(bool force)
    {
        foreach (var item in bundles.Values)
            item.bundle.Unload(true);
        if (force)
            mainfestBundle.Unload(true);

        bundles.Clear();
    }
}


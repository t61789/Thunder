using System;
using System.IO;
using Thunder.Utility;
using Tool;

namespace Tool
{
    public static class Config
    {
        /// <summary>
        /// 用于初始化对象
        /// </summary>
        /// <returns>需要加入核心生命周期的继承了IBaseSys的对象</returns>
        public static IBaseSys[] Init()
        {
            RandomRewardGenerator.ConstractDic(DataBaseSys.Ins["reward"]);

            return new IBaseSys[]
            {
                new ItemSys()
            };
        }
    }

    /// <summary>
    /// 指定游戏所需的各个路径
    /// </summary>
    public class Paths
    {
        public static char Div = '/';   // 路径分隔符

        public static string DocumentPath;  // 在静态构造方法中更改

#if UNITY_EDITOR
        public static string BundleBasePath = @"E:\AssetBundles" + Div + "StandaloneWindows";
        public static string LogPath = @"E:\ThunderLog.txt";
#elif UNITY_STANDALONE
        public static string BundleBasePath = @"."+Div+"StandaloneWindows";
        public static string LogPath = @"."+Div+"ThunderLog.txt";
#elif UNITY_ANDROID && !UNITY_EDITOR
        public static string BundleBasePath = Application.persistentDataPath+@"Bundles";
        public static string LogPath = Application.persistentDataPath+"ThunderLog.txt";
#endif

        public static readonly string DllBundle = @"dll";
        public static readonly string PrefabBundle = @"prefabs";
        public static readonly string UIBundle = @"ui";
        public static readonly string ValuesBundle = @"values";
        public static readonly string DatabaseBundle = @"database";
        public static readonly string LuaBundle = @"lua";
        public static readonly string Normal = @"normal";

        static Paths()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            DocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                           Path.DirectorySeparatorChar +
                           "MyGames" +
                           Path.DirectorySeparatorChar +
                           "Thunder";
#elif UNITY_ANDROID && !UNITY_EDITOR
            DocumentPath = Application.persistentDataPath +Div +"Doc";
#endif
            DocumentPathD = DocumentPath + Div;

            // 创建存档路径和bundle组路径
            if (!Directory.Exists(DocumentPath))
                Directory.CreateDirectory(DocumentPath);
            if (!Directory.Exists(BundleBasePath))
                Directory.CreateDirectory(BundleBasePath);
        }

        // 这些末尾为D路径的代表在原路径的基础上加上了分隔符
        public static string DocumentPathD;
        public static string BundleBasePathD = BundleBasePath + Div;
        public static readonly string DllBundleD = DllBundle + Div;
        public static readonly string PrefabBundleD = PrefabBundle + Div;
        public static readonly string UIBundleD = UIBundle + Div;
        public static readonly string ValuesBundleD = ValuesBundle + Div;
        public static readonly string DatabaseBundleD = DatabaseBundle + Div;
        public static readonly string LuaBundleD = LuaBundle + Div;
        public static readonly string NormalD = Normal + Div;
    }
}

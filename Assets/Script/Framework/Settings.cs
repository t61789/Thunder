using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 指定游戏所需的各个路径
    /// </summary>
    public class Paths
    {
        public static char Div = '\\';   // 默认路径分隔符

        public static string DocumentPath;
        public static string BundleBasePath;
        public static string LogPath;

        public static string DllBundle;
        public static string PrefabBundle;
        public static string UIBundle;
        public static string ValuesBundle;
        public static string DatabaseBundle;
        public static string LuaBundle;
        public static string Normal = "normal";

        static Paths()
        {
            LoadFromXml(Config.ConfigXmlPath);

            if (!Directory.Exists(DocumentPath))
                Directory.CreateDirectory(DocumentPath);
        }

        private static void LoadFromXml(string path)
        {
            var root = XDocument.Load(path).Root;

            // 反射填充bundle名
            var t = typeof(Paths);
            foreach (var e in root.Element("SysBundleName").Elements())
                t.GetField(e.Name.ToString(), BindingFlags.Static | BindingFlags.Public)
                    .SetValue(null, e.Attribute("name").Value);

            // 处理相对路径
            string platformName;
            var platform = Platform.CurPlatform;
            if (platform.HasFlag(Platforms.Standalone) || platform.HasFlag(Platforms.Editor))
            {
                DocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                BundleBasePath = Application.dataPath;
                platformName = "Standalone";
            }
            else if (platform.HasFlag(Platforms.Android))
            {
                DocumentPath = Application.persistentDataPath;
                BundleBasePath = DocumentPath;
                platformName = "Android";
            }
            else
            {
                throw new Exception("不支持当前平台");
            }
            LogPath = BundleBasePath;

            var el = root.Element("PlatformRelative").Element(platformName);
            DocumentPath = DocumentPath.PCombine(el.Element("Document").Attribute("path").Value);
            BundleBasePath = BundleBasePath.PCombine(el.Element("Bundle").Attribute("path").Value);
            LogPath = LogPath.PCombine(el.Element("Log").Attribute("path").Value);

            // 处理强制路径
            el = root.Element(nameof(BundleBasePath));
            if (el.Attribute("enable").Value == "true")
                BundleBasePath = el.Attribute("path").Value.Replace(Div, Path.DirectorySeparatorChar);
            el = root.Element(nameof(LogPath));
            if (el.Attribute("enable").Value == "true")
                LogPath = el.Attribute("path").Value.Replace(Div, Path.DirectorySeparatorChar);
        }
    }
    public class Platform
    {
        public static Platforms CurPlatform;

        static Platform()
        {
            ResolveCurPlatform();
        }

        private static void ResolveCurPlatform()
        {
            CurPlatform = 0;
#if UNITY_EDITOR
            CurPlatform |= Platforms.Editor;
#endif
#if UNITY_ANDROID
            CurPlatform |= Platforms.Android;
#endif
#if UNITY_STANDALONE
            CurPlatform |= Platforms.Standalone;
#endif
#if UNITY_STANDALONE_WIN
            CurPlatform |= Platforms.StandaloneWin;
#endif
        }

        public static bool IsStandalone()
        {
            return CurPlatform.HasFlag(Platforms.Editor) ||
                   CurPlatform.HasFlag(Platforms.StandaloneWin) ||
                   CurPlatform.HasFlag(Platforms.Standalone);
        }
    }

    [Flags]
    public enum Platforms
    {
        Standalone = 1,
        Editor = 1 << 1,
        Android = 1 << 2,
        StandaloneWin = 1 << 3,
    }
}

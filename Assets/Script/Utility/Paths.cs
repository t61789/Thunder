using System;
using System.IO;

namespace Thunder.Utility
{
    public class Paths
    {
        public static string DocumentPath;
        public static string DocumentPathD;
        public static char Div = '/';
        

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
        public static string BundleBasePathD = BundleBasePath + Div;

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

            if (!Directory.Exists(DocumentPath))
                Directory.CreateDirectory(DocumentPath);
            if (!Directory.Exists(BundleBasePath))
                Directory.CreateDirectory(BundleBasePath);
        }
    }

    public class Fuck
    {
        private int a;
    }
}

using System;
using System.IO;

namespace Thunder.Utility
{
    public class Paths
    {
        public static string DocumentPath;
        public static string DocumentPathD;
        public static char Div = '/';
        public static string LogPath = @"E:\ThunderLog.txt";

#if UNITY_STANDALONE || UNITY_EDITOR
        public static string BundleBasePath = @"E:\AssetBundles" + Div + "StandaloneWindows";
#elif UNITY_ANDROID && !UNITY_EDITOR
    public static string BundleBasePath = Application.persistentDataPath+@"Bundles";
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

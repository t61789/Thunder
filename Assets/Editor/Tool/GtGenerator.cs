using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Tool
{
    [ExecuteInEditMode]
    public class GtGenerator
    {
        private static List<int> li = new List<int> { 1, 2, 3, S() };
        static GtGenerator()
        {
            Debug.Log(1);
        }

        [MenuItem("Shit/fffgg")]
        public static void Generate()
        {
            foreach (var type in typeof(Thunder.Bullet).Assembly.GetTypes().Where(x =>
                x.Namespace != null &&
                x.Namespace.StartsWith("Thunder") &&
                !x.IsNested))
            {
                Debug.Log(type.FullName);
            }
        }

        public static int S()
        {
            return 12;
        }
    }
}

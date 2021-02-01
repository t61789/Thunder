using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Thunder
{
    public class GetChildPos
    {
        [MenuItem("Tools/GetChildPos")]
        public static void CheckBehaviorProperties()
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                var sb = new StringBuilder();
                foreach (var pos in from Transform child in gameObject.transform select child.position)
                {
                    sb.Append("{\"x\":");
                    sb.Append(pos.x);
                    sb.Append(",\"y\":");
                    sb.Append(pos.y);
                    sb.Append(",\"z\":");
                    sb.Append(pos.z);
                    sb.Append("},\n");
                }
                Debug.Log(sb.ToString());
                return;
            }
        }
    }
}

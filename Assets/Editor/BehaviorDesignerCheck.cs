using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviorDesigner.Runtime;
using Thunder.Behavior;
using UnityEditor;
using UnityEngine;

namespace Thunder
{
    public class BehaviorDesignerCheck
    {
        [MenuItem("Tools/Check behavior properties")]
        public static void CheckBehaviorProperties()
        {
            BehaviorTree tree = null;
            foreach (var gameObject in Selection.gameObjects)
            {
                tree = gameObject.GetComponent<BehaviorTree>();
                if (tree != null) break;
            }
            if (tree == null) return;

            Debug.Log("Start checking properties");
            foreach (var delegateInvoke in tree.FindTasks<DelegateInvoke>())
                delegateInvoke.TestProperties();
        }
    }
}

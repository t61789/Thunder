using System.Collections.Generic;
using UnityEngine;

namespace Thunder.Tool.BehaviorTree
{
    public class BehaviorTree : MonoBehaviour
    {
        public List<Node> nodes;
        public Node firstNode;

        void FixedUpdate()
        {
            Process();
        }

        public void ClearNodes()
        {
            firstNode = null;
            nodes.Clear();
            global::System.GC.Collect();
        }

        public void AddNode(Node node, int parentId)
        {
            if (firstNode == null)
                firstNode = node;
            if (nodes.Exists(x => x.id == node.id))
            {
                Debug.LogError("Id " + node.id + "已存在");
                return;
            }
            nodes.Add(node);
            nodes.Find(x => x.id == parentId)?.child.Add(node);
        }

        private void Process()
        {
            if (firstNode == null)
                return;

            List<Node> stack = new List<Node>() { firstNode };
            while (stack.Count != 0)
            {
                Node newNode = stack[stack.Count - 1].Action();
                if (newNode == null)
                {
                    if (stack.Count > 1)
                        stack[stack.Count - 2].ReciveReturnResult(stack[stack.Count - 1].ReturnResult());
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                    stack.Add(newNode);
            }
        }
    }
}

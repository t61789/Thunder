using System.Collections.Generic;

namespace Thunder.Tool.BehaviorTree
{
    public class Node
    {
        public List<Node> child;
        public int id;

        public Node(int id)
        {
            this.id = id;
        }

        public virtual Node Action()
        {
            return null;
        }

        public virtual bool ReturnResult()
        {
            return true;
        }

        public virtual void ReciveReturnResult(bool value)
        {
        }
    }
}
using System.Collections.Generic;

namespace Tool.BehaviorTree
{
    public class Node
    {
        public int id;

        public List<Node> child;

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

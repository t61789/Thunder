namespace Tool.BehaviorTree
{
    public class SelectorNode : Node
    {
        public int curIndex = 0;

        public SelectorNode(int id) : base(id)
        {

        }

        public override Node Action()
        {
            if (curIndex < child.Count)
                return child[curIndex];
            else
                return null;
        }

        public override bool ReturnResult()
        {
            curIndex = 0;
            return true;
        }

        public override void ReciveReturnResult(bool value)
        {
            if (value)
                curIndex = int.MaxValue;
            else
                curIndex++;
        }
    }
}

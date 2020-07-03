namespace Assets.Script.Tool.BehaviorTree
{
    public class ConditionNode : Node
    {
        public delegate bool DelCondition();
        public DelCondition condition;

        public bool result = false;

        public bool actionOver = false;

        public ConditionNode(int id, DelCondition condition) : base(id)
        {
            this.condition = condition;
        }

        public override Node Action()
        {
            if (actionOver) return null;

            result = condition();
            if (result)
                foreach (var i in child)
                    return i;

            return null;
        }

        public override bool ReturnResult()
        {
            actionOver = false;
            return result;
        }

        public override void ReciveReturnResult(bool value)
        {
            actionOver = true;
        }
    }
}

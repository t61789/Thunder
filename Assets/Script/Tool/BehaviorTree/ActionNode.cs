namespace Assets.Script.Tool.BehaviorTree
{
    public class ActionNode : Node
    {
        public delegate void DelAction();
        public DelAction action;

        public ActionNode(int id, DelAction action) : base(id)
        {
            this.action = action;
        }

        public override Node Action()
        {
            action();
            return null;
        }
    }
}

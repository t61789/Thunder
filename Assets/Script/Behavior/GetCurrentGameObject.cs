using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Thunder.Behavior
{
    public class GetCurrentGameObject : Action
    {
        public SharedGameObject curGameObject;

        public override TaskStatus OnUpdate()
        {
            curGameObject.Value = gameObject;
            return TaskStatus.Success;
        }
    }
}
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Assets.Script.Behavior
{
    [TaskCategory("Unity/GameObject")]
    public class IsNotNull : Conditional
    {
        public SharedGameObject go;

        public override TaskStatus OnUpdate()
        {
            if (go.Value == null)
                return TaskStatus.Failure;
            else
                return TaskStatus.Success;
        }
    }
}

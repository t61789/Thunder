using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace Thunder.Behavior
{
    public class GetDistance : Action
    {
        public SharedFloat distance;
        public SharedGameObject obj1;
        public SharedGameObject obj2;

        public override TaskStatus OnUpdate()
        {
            try
            {
                distance.Value = (obj1.Value.transform.position - obj2.Value.transform.position).magnitude;
                return TaskStatus.Success;
            }
            catch (Exception)
            {
                return TaskStatus.Failure;
                throw;
            }
        }
    }
}
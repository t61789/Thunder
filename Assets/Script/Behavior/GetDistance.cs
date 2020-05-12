using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class GetDistance : Action
{
    public SharedGameObject obj1;
    public SharedGameObject obj2;

    public SharedFloat distance;

    public override TaskStatus OnUpdate()
    {
        try
        {
            distance.Value = (obj1.Value.transform.position - obj2.Value.transform.position).magnitude;
            return TaskStatus.Success;
        }
        catch (System.Exception)
        {
            return TaskStatus.Failure;
            throw;
        }
    }
}

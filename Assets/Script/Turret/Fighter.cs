using BehaviorDesigner.Runtime.Tasks;
using Tool.BuffData;
using UnityEngine;

public class Fighter : Aircraft
{
    public float MakeInterval = 2f;

    public bool GuardClock = true;
    public float GuardDistance = 5;
    public float GuardFollowDistance = 7;
    public float GuardAngle = 25;
    public float ArrivePositionDistance = 0.5f;
    public float AttackAreaDistance = 5;
    public float TooCloseToTargetDistance = 2;
    public float SlowCurise = 0.5f;
    public float AcceleraionAngle = 60;
    public int PathPosSize = 3;
    public float patrolRange = 5;

    protected float guardDistance;
    protected float arrivePositionDistance;
    protected float attackAreaDistance;
    protected float tooCloseToTargetDistance;
    protected BuffData slowCurise;
    protected float sqrDistance;
    protected CircleCollider2D searchBox;
    protected Vector3 stayPos;

    protected override void Awake()
    {
        base.Awake();
        guardDistance = GuardDistance * GuardDistance;
        arrivePositionDistance = ArrivePositionDistance * ArrivePositionDistance;
        attackAreaDistance = AttackAreaDistance * AttackAreaDistance;
        tooCloseToTargetDistance = TooCloseToTargetDistance * TooCloseToTargetDistance;
        slowCurise = SlowCurise;
        TurnOnSlowCruise();
        searchBox = GetComponent<CircleCollider2D>();
        searchBox.enabled = false;
    }

    public TaskStatus TurnOffSlowCruise()
    {
        accelerationFront.RemoveBuff("slowCurise");
        return TaskStatus.Success;
    }

    public TaskStatus ClearPathPos()
    {
        pathPos.Clear();
        return TaskStatus.Success;
    }

    public TaskStatus TargetExists()
    {
        return Target ? TaskStatus.Success : TaskStatus.Failure;
    }

    public TaskStatus InitPathPos()
    {
        Vector3 start = (trans.position - guardTargetTrans.position).normalized * GuardDistance + guardTargetTrans.position;
        pathPos.Add(start);
        for (int i = 0; i < PathPosSize - 1; i++)
        {
            AddNewGuardPos();
        }
        return TaskStatus.Success;
    }

    public TaskStatus GuardTargetExists()
    {
        return GuardTarget ? TaskStatus.Success : TaskStatus.Failure;
    }

    public TaskStatus SetTargetPathPosition()
    {
        pathPos.Add(targetTrans.position);
        return TaskStatus.Success;
    }

    public TaskStatus TurnOnSlowCruise()
    {
        accelerationFront.AddBuff(2, slowCurise, 0, "slowCurise");
        return TaskStatus.Success;
    }

    public TaskStatus DistanceToGuardTarget()
    {
        return (guardTargetTrans.position - trans.position).sqrMagnitude > guardDistance ? TaskStatus.Success : TaskStatus.Failure;
    }

    public TaskStatus PathPosLessThanLimit()
    {
        return pathPos.Count < PathPosSize ? TaskStatus.Success : TaskStatus.Failure;
    }

    public TaskStatus AddNewGuardPos()
    {
        pathPos.Add(Quaternion.AngleAxis(GuardAngle, GuardClock ? Vector3.back : Vector3.forward) * (pathPos[pathPos.Count - 1] - guardTargetTrans.position).normalized * GuardDistance + guardTargetTrans.position);
        return TaskStatus.Success;
    }

    public TaskStatus TurnToPathPos()
    {
        StaringAtControl = pathPos[0];
        return TaskStatus.Success;
    }

    public TaskStatus TargetInAttackArea()
    {
        return (targetTrans.position - trans.position).sqrMagnitude < attackAreaDistance ? TaskStatus.Success : TaskStatus.Failure;
    }

    public TaskStatus TooCloseToTarget()
    {
        return (targetTrans.position - trans.position).sqrMagnitude < tooCloseToTargetDistance ? TaskStatus.Success : TaskStatus.Failure;
    }

    public TaskStatus MoveBack()
    {
        SpeedUpBackControl = true;
        return TaskStatus.Success;
    }

    public TaskStatus GetSqrDistance()
    {
        sqrDistance = (pathPos[0] - trans.position).sqrMagnitude;
        return TaskStatus.Success;
    }

    public TaskStatus LastPathPoint()
    {
        return pathPos.Count == 1 ? TaskStatus.Success : TaskStatus.Failure;
    }

    public TaskStatus NeedToSlowDown()
    {
        return sqrDistance <= rb2d.velocity.sqrMagnitude / (Drag * Drag) ? TaskStatus.Success : TaskStatus.Failure;
    }

    public TaskStatus AngleLess()
    {
        return Vector3.Angle(trans.rotation * Vector3.up, pathPos[0] - trans.position) <= AcceleraionAngle ? TaskStatus.Success : TaskStatus.Failure;
    }

    public TaskStatus SpeedUp()
    {
        SpeedUpFrontControl = true;
        return TaskStatus.Success;
    }

    public TaskStatus ArrivePathPosition()
    {
        return sqrDistance < arrivePositionDistance ? TaskStatus.Success : TaskStatus.Failure;
    }

    public TaskStatus DeletePathPosition()
    {
        pathPos.RemoveAt(0);
        return TaskStatus.Success;
    }

    public TaskStatus TurnToTarget()
    {
        StaringAtControl = targetTrans.position;
        return TaskStatus.Success;
    }

    public TaskStatus AddTargetPathPos()
    {
        if (pathPos.Count == 0)
            pathPos.Add(targetTrans.position);
        else
            pathPos[0] = targetTrans.position;
        return TaskStatus.Success;
    }

    public TaskStatus PathPosExists()
    {
        return pathPos.Count != 0 ? TaskStatus.Success : TaskStatus.Failure;
    }

    public TaskStatus OpenSearchBox()
    {
        searchBox.enabled = true;
        return TaskStatus.Success;
    }

    public TaskStatus CloseSearchBox()
    {
        searchBox.enabled = false;
        return TaskStatus.Success;
    }

    public TaskStatus RecordCurPos()
    {
        stayPos = trans.position;
        return TaskStatus.Success;
    }

    public TaskStatus AddNewPatrolPos()
    {
        for (int i = pathPos.Count; i < PathPosSize; i++)
            pathPos.Add(Tool.Tools.RandomVectorInCircle(patrolRange) + stayPos);
        return TaskStatus.Success;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        if (GuardTarget != null) return;

        Aircraft camp = other.GetComponent<Aircraft>();
        if (camp == null) return;
        if (!PublicVar.camp.IsHostile(camp, this)) return;

        SetTarget(camp as Aircraft);
    }

    //public void OnDrawGizmos()
    //{
    //	foreach (var item in pathPos)
    //	{
    //		Gizmos.DrawLine(trans.position, item);
    //	}

    //	Gizmos.DrawWireSphere(guardTargetTrans.position, GuardDistance);
    //	Gizmos.DrawWireSphere(guardTargetTrans.position, GuardFollowDistance);
    //}
}

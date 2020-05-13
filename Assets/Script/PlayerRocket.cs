using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRocket : Bullet
{
    public int HangTime;
    [Range(0, 1)]
    public float InheritVelocityPercent;
    public float FrontAccleration;
    [Range(1.01f, 4)]
    public float AcclerationCoefficient;
    public float TurnDegree;

    private float startEngineCount;
    private Transform target;
    private SortedList<float, Transform> targetList = new SortedList<float, Transform>();
    //private EnemyController contactEnemy;
    private Rigidbody2D playerRb2d;

    private float detectRange;
    private CircleCollider2D detectCollider2d;
    private BoxCollider2D contactCollider2d;
    private BehaviorTree behaviorTree;

    protected new void Awake()
    {
        base.Awake();
        detectCollider2d = GetComponent<CircleCollider2D>();
        contactCollider2d = GetComponent<BoxCollider2D>();
        detectRange = (detectCollider2d.radius / 2) * (detectCollider2d.radius / 2);
        behaviorTree = GetComponent<BehaviorTree>();
        behaviorTree.SetVariableValue("controller", this);
    }

    protected TaskStatus StartEngineFinished()
    {
        return Time.time - startEngineCount >= (HangTime / 1000f) ? TaskStatus.Success : TaskStatus.Failure;
    }

    protected TaskStatus TargetExists()
    {
        return target == null ? TaskStatus.Failure : TaskStatus.Success;
    }

    //protected TaskStatus ContactedEnemy()
    //{
    //    return contactEnemy != null ? TaskStatus.Success : TaskStatus.Failure;
    //}

    protected TaskStatus StartExplode()
    {
        //contactEnemy.GetDamage(Damage);
        rb2d.velocity = Vector2.zero;
        animator.SetBool("contactEnemy", true);
        behaviorTree.enabled = false;
        return TaskStatus.Running;
    }

    protected TaskStatus EnemyExist()
    {
        return target != null ? TaskStatus.Success : TaskStatus.Failure;
    }

    protected TaskStatus TurnToTarget()
    {
        Vector3 temp = target.transform.position - transform.position;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(Vector3.up, temp), TurnDegree);
        return TaskStatus.Success;
    }

    protected TaskStatus IsContactCollider()
    {
        return contactCollider2d.enabled ? TaskStatus.Success : TaskStatus.Failure;
    }

    protected TaskStatus SetContactCollider()
    {
        SetCollider(false);
        return TaskStatus.Success;
    }

    protected TaskStatus DistanceJudge()
    {
        return (target.transform.position - transform.position).sqrMagnitude > detectRange ? TaskStatus.Success : TaskStatus.Failure;
    }

    protected TaskStatus SetTargetEmpty()
    {
        target = null;
        return TaskStatus.Success;
    }

    protected TaskStatus SetSearchCollider()
    {
        SetCollider(true);
        return TaskStatus.Success;
    }

    private void SpeedUp()
    {
        Vector3 to = transform.rotation * Vector3.up;
        Vector3 tempVelocity = rb2d.velocity;
        float speed = Vector3.Dot(to, tempVelocity);

        float newAccletation = FrontAccleration * (1 / (1 - speed / MaxSpeed / AcclerationCoefficient));

        tempVelocity += to * newAccletation;
        if (tempVelocity.magnitude > MaxSpeed)
            tempVelocity = tempVelocity.normalized * MaxSpeed;

        rb2d.velocity = tempVelocity;
    }

    public override void ObjectPoolReset()
    {
        //base.ObjectPoolReset(arg);
        //startEngineCount = Time.time;
        //SetCollider(true);
        //animator.Play("playerRocketIdle");
        //animator.SetBool("contactEnemy", false);
        //contactEnemy = null;
        //target = null;
        //behaviorTree.enabled = true;
        //playerRb2d = releaser.GetComponent<Rigidbody2D>();
        //rb2d.velocity = playerRb2d.velocity * InheritVelocityPercent;
    }

    protected void Exploded()
    {
        PublicVar.objectPool.Recycle(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (contactCollider2d.enabled)
        //{
        //    contactEnemy = collision.GetComponent<EnemyController>();
        //}
        //else
        //{
        //    if (collision.GetComponent<EnemyController>() == null)
        //        return;
        //    Transform curTrans = collision.transform;
        //    targetList.Add((curTrans.position - transform.position).sqrMagnitude, curTrans);
        //}
    }

    //private void LateUpdate()
    //{
    //    if (target != null)
    //        return;
    //    if (targetList.Count != 0)
    //    {
    //        target = targetList.Values[0];
    //        SetCollider(false);
    //        targetList.Clear();
    //    }
    //}

    private void SetCollider(bool detect)
    {
        detectCollider2d.enabled = detect;
        contactCollider2d.enabled = !detect;
    }
}

using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using Tool.BuffData;
using Tool.ObjectPool;
using UnityEngine;

public class Aircraft : Controller, IObjectPool
{
    public bool DrawPathPos;

    public bool Controllable = true;
    public bool ForceTurn = false;
    public float Drag = 5;
    public float TurnDegree = 110;
    public string Camp;
    public float AccelerationFront = 20;
    public float AccelerationBack = 10;
    public float AccelerationLeft = 5;
    public float AccelerationRight = 5;

    public float MaxHealth = 100;

    public BuffData drag;

    public Aircraft Target;
    public Aircraft GuardTarget;

    protected float Health;

    protected Transform targetTrans;
    protected Transform guardTargetTrans;
    protected BuffData maxSpeed;
    protected Rigidbody2D rb2d;
    protected Transform trans;
    protected BuffData accelerationFront;
    protected BuffData accelerationBack;
    protected BuffData accelerationLeft;
    protected BuffData accelerationRight;

    protected List<Vector3> pathPos = new List<Vector3>();

    public delegate void DestroyedDel(Aircraft aircraft);
    public event DestroyedDel OnDestroyed;

    protected bool SpeedUpFrontControl { get; set; }
    public void SetSpeedUpFrontControl(bool b)
    {
        SpeedUpFrontControl = b;
    }
    protected bool SpeedUpBackControl { get; set; }
    public void SetSpeedUpBackControl(bool b)
    {
        SpeedUpBackControl = b;
    }
    protected bool SpeedUpLeftControl { get; set; }
    public void SetSpeedUpLeftControl(bool b)
    {
        SpeedUpLeftControl = b;
    }
    protected bool SpeedUpRightControl { get; set; }
    public void SetSpeedUpRightControl(bool b)
    {
        SpeedUpRightControl = b;
    }
    protected bool TurnRightControl { get; set; }
    public void SetTurnRightControl(bool b)
    {
        TurnRightControl = b;
    }
    protected bool TurnLeftControl { get; set; }
    public void SetTurnLeftControl(bool b)
    {
        TurnLeftControl = b;
    }
    protected Vector3 StaringAtControl { get; set; } = Vector3.positiveInfinity;
    public void SetStaringAtControl(Vector3 v)
    {
        StaringAtControl = v;
    }

    protected override void Awake()
    {
        base.Awake();
        rb2d = GetComponent<Rigidbody2D>();
        trans = transform;
        Health = MaxHealth;
        drag = Drag;
        maxSpeed = Mathf.Max(AccelerationLeft, AccelerationRight, AccelerationFront, AccelerationBack);
        maxSpeed.AddBuff(3, drag, 0, "dragCoefficient");
        accelerationFront = AccelerationFront;
        accelerationLeft = AccelerationLeft;
        accelerationBack = AccelerationBack;
        accelerationRight = AccelerationRight;
        SetTarget(Target);
        SetGuardTarget(GuardTarget);
        GetComponent<BehaviorTree>()?.SetVariableValue("controller",this);
    }

    public void GetDamage(float damage)
    {
        Health -= damage;
        if (Health <= 0)
            Dead();
    }

    public void Dead()
    {
        OnDestroyed?.Invoke(this);
    }

    protected void FixedUpdate()
    {
        if (Controllable)
        {
            Vector3 curPos = trans.position;
            Quaternion curRot = trans.rotation;

            if (StaringAtControl.Equals(Vector3.positiveInfinity))
            {
                if (TurnRightControl)
                {
                    curRot = TurnRight(curRot);
                    TurnRightControl = false;
                }
                if (TurnLeftControl)
                {
                    curRot = Turnleft(curRot);
                    TurnLeftControl = false;
                }
            }
            else
            {
                curRot = StaringAt(curRot, StaringAtControl, curPos);
                StaringAtControl = Vector3.positiveInfinity;
            }
            trans.rotation = curRot;


            Vector3 curDir = (curRot * Vector3.up).normalized;
            Vector3 curVelocity = rb2d.velocity;

            curVelocity -= drag * Time.fixedDeltaTime * rb2d.velocity.magnitude * curVelocity.normalized;

            if (SpeedUpFrontControl)
            {
                curVelocity = SpeedUpFront(curVelocity, curDir);
                SpeedUpFrontControl = false;
            }

            if (SpeedUpBackControl)
            {
                curVelocity = SpeedUpBack(curVelocity, curDir);
                SpeedUpBackControl = false;
            }

            if (SpeedUpLeftControl)
            {
                curVelocity = SpeedUpLeft(curVelocity, curDir);
                SpeedUpLeftControl = false;
            }

            if (SpeedUpRightControl)
            {
                curVelocity = SpeedUpRight(curVelocity, curDir);
                SpeedUpRightControl = false;
            }

            if (ForceTurn)
                curVelocity = trans.rotation * (Quaternion.FromToRotation(curVelocity, Vector3.up) * curVelocity);

            rb2d.velocity = curVelocity;
        }
    }

    protected virtual Quaternion StaringAt(Quaternion curRot, Vector3 staringPos, Vector3 curPos)
    {
        return Quaternion.RotateTowards(curRot, Quaternion.FromToRotation(Vector3.up, staringPos - curPos), TurnDegree * Time.fixedDeltaTime);
    }

    protected virtual Quaternion TurnRight(Quaternion curRot)
    {
        Vector3 temp = Vector3.Cross(Vector3.back, curRot * Vector3.up);
        return Quaternion.RotateTowards(curRot, Quaternion.FromToRotation(Vector3.up, temp), TurnDegree * Time.fixedDeltaTime);
    }

    protected virtual Quaternion Turnleft(Quaternion curRot)
    {
        Vector3 temp = Vector3.Cross(Vector3.forward, curRot * Vector3.up);
        return Quaternion.RotateTowards(curRot, Quaternion.FromToRotation(Vector3.up, temp), TurnDegree * Time.fixedDeltaTime);
    }

    protected virtual Vector3 SpeedUpFront(Vector3 curVelocity, Vector3 curDir)
    {
        return curVelocity + curDir * accelerationFront * Time.fixedDeltaTime;
    }

    protected virtual Vector3 SpeedUpBack(Vector3 curVelocity, Vector3 curDir)
    {
        return curVelocity + curDir * accelerationBack * Time.fixedDeltaTime;
    }

    protected virtual Vector3 SpeedUpLeft(Vector3 curVelocity, Vector3 curDir)
    {
        return curVelocity + Vector3.Cross(Vector3.forward, curDir) * accelerationLeft * Time.fixedDeltaTime;
    }

    protected virtual Vector3 SpeedUpRight(Vector3 curVelocity, Vector3 curDir)
    {
        return curVelocity + Vector3.Cross(curDir, Vector3.forward) * accelerationRight * Time.fixedDeltaTime;
    }

    public void SetTarget(Aircraft target)
    {
        Target = target;
        targetTrans = target?.transform;
    }

    public void SetGuardTarget(Aircraft guardTarget)
    {
        GuardTarget = guardTarget;
        guardTargetTrans = guardTarget?.transform;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public virtual void ObjectPoolReset()
    {
        pathPos.Clear();
    }

    public virtual void ObjectPoolRecycle()
    {

    }

    public virtual void ObjectPoolDestroy()
    {
        Destroy(gameObject);
    }
    
    public virtual void ObjectPoolInit(Vector3 position,Quaternion rotation,Aircraft target,Aircraft guardTarget,string camp=null)
    {
        trans.position = position;
        trans.rotation = rotation;
        Camp = camp;
        SetTarget(target);
        SetGuardTarget(guardTarget);
    }

    public void OnDrawGizmos()
    {
        if (DrawPathPos)
        {
            Vector3 left = trans.position;
            foreach (var item in pathPos)
            {
                Gizmos.DrawLine(left,item);
                left = item;
            }
        }
    }
}

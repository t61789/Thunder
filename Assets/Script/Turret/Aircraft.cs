using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using Tool.BuffData;
using Tool.ObjectPool;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Aircraft : Controller, IObjectPool
{
    public enum ControlTypes
    {
        absoluteJoystick,
        relativeJoystick
    }

    public bool DrawPathPos;

    public bool Controllable = true;
    public bool ForceTurn = false;

    [Space]
    public ControlTypes ControlType = ControlTypes.absoluteJoystick;
    public float AccFrontAngle = 30;
    public float TranslationAngle = 60;
    public float TurnAngle = 150;
    public float AimmingPosAcce = 10;
    public float AimmingPosRange = 10;
    [Space]

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

    [HideInInspector]
    public Transform trans;
    protected float Health;
    protected Transform targetTrans;
    protected Transform guardTargetTrans;
    protected BuffData maxSpeed;
    protected Rigidbody2D rb2d;
    protected BuffData accelerationFront;
    protected BuffData accelerationBack;
    protected BuffData accelerationLeft;
    protected BuffData accelerationRight;

    protected List<Vector3> pathPos = new List<Vector3>();
    [HideInInspector]
    public Vector3 aimmingPos;

    public delegate void DeadDel(Aircraft aircraft);
    public event DeadDel OnDead;

    //控制变量
    protected bool SpeedUpFrontControl;
    public void SetSpeedUpFrontControl(bool b)
    {
        SpeedUpFrontControl = b;
    }
    protected bool SpeedUpBackControl;
    public void SetSpeedUpBackControl(bool b)
    {
        SpeedUpBackControl = b;
    }
    protected bool SpeedUpLeftControl;
    public void SetSpeedUpLeftControl(bool b)
    {
        SpeedUpLeftControl = b;
    }
    protected bool SpeedUpRightControl;
    public void SetSpeedUpRightControl(bool b)
    {
        SpeedUpRightControl = b;
    }
    protected bool TurnRightControl;
    public void SetTurnRightControl(bool b)
    {
        TurnRightControl = b;
    }
    protected bool TurnLeftControl;
    public void SetTurnLeftControl(bool b)
    {
        TurnLeftControl = b;
    }
    protected Vector3 StaringAtControl = Vector3.positiveInfinity;
    public void SetStaringAtControl(Vector3 v)
    {
        StaringAtControl = v;
    }
    protected Vector3 AccelerationVector = Vector3.positiveInfinity;
    public void SetAccelerationVector(Vector3 v)
    {
        AccelerationVector = v;
    }
    protected Vector3 JoystickDir;
    public void SetJoystickDir(Vector3 dir)
    {
        JoystickDir = dir;
    }
    protected Vector3 ShootingJoystick;
    public void SetShootingJoystick(Vector3 dir)
    {
        ShootingJoystick = dir;
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
        GetComponent<BehaviorTree>()?.SetVariableValue("controller", this);
    }

    public void GetDamage(float damage)
    {
        Health -= damage;
        if (Health <= 0)
            Dead();
    }

    public void Dead()
    {
        OnDead?.Invoke(this);
    }

    protected void FixedUpdate()
    {
        if (Controllable)
        {
            if (!JoystickDir.Equals(Vector3.positiveInfinity))
                ParseDir(JoystickDir);

            if (!ShootingJoystick.Equals(Vector3.positiveInfinity))
                aimmingPos += ShootingJoystick * AimmingPosAcce * Time.fixedDeltaTime;

            Vector3 curPos = trans.position;
            Quaternion curRot = trans.rotation;

            if (StaringAtControl.Equals(Vector3.positiveInfinity))
            {
                if (TurnRightControl)
                    curRot = TurnRight(curRot);
                if (TurnLeftControl)
                    curRot = Turnleft(curRot);
            }
            else
                curRot = StaringAt(curRot, StaringAtControl, curPos);
            trans.rotation = curRot;

            Vector3 curDir = (curRot * Vector3.up).normalized;
            Vector3 curVelocity = rb2d.velocity;

            curVelocity -= drag * Time.fixedDeltaTime * rb2d.velocity.magnitude * curVelocity.normalized;

            if (SpeedUpFrontControl)
                curVelocity = SpeedUpFront(curVelocity, curDir);

            if (SpeedUpBackControl)
                curVelocity = SpeedUpBack(curVelocity, curDir);

            if (SpeedUpLeftControl)
                curVelocity = SpeedUpLeft(curVelocity, curDir);

            if (SpeedUpRightControl)
                curVelocity = SpeedUpRight(curVelocity, curDir);

            if (ForceTurn)
                curVelocity = trans.rotation * (Quaternion.FromToRotation(curVelocity, Vector3.up) * curVelocity);

            rb2d.velocity = curVelocity;
        }
    }

    private void ParseDir(Vector3 joystickDir)
    {
        if (joystickDir.Equals(Vector3.zero)) return;

        Vector3 baseDir;
        if (ControlType == ControlTypes.absoluteJoystick)
            baseDir = trans.rotation * Vector3.up;
        else
            baseDir = Vector3.up;

        float angle = Vector3.Angle(baseDir, joystickDir);
        if (angle <= AccFrontAngle)
        {
            SpeedUpFrontControl = true;
        }
        else if (angle <= TranslationAngle && !ForceTurn)
        {
            Vector3 temp = Vector3.Cross(baseDir, joystickDir);
            if (temp.z < 0)
                SpeedUpRightControl = true;
            else
                SpeedUpLeftControl = true;
        }
        else if (angle <= TurnAngle || ForceTurn)
        {
            Vector3 temp = Vector3.Cross(baseDir, joystickDir);
            if (temp.z < 0)
                TurnRightControl = true;
            else
                TurnLeftControl = true;
        }
        else
            SpeedUpBackControl = true;
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

    public virtual void ObjectPoolInit(Vector3 position, Quaternion rotation, Aircraft target, Aircraft guardTarget, string camp = null)
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
                Gizmos.DrawLine(left, item);
                left = item;
            }
        }
    }
}

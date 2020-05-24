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

    protected const string ACC_FRONT = "AccFront";
    protected const string ACC_BACK = "AccBack";
    protected const string ACC_LEFT = "AccLeft";
    protected const string ACC_RIGHT = "AccRight";
    protected const string ACC_DIR = "AccDir";
    protected const string TURN_RIGHT = "TurnRight";
    protected const string TURN_LEFT = "TurnLeft";
    protected const string STARING_AT = "StaringAt";
    protected const string JOYSTICK = "Joystick";
    protected const string SHOOTING_JOYSTICK = "ShootingJoystick";

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
            
            Vector3 tempControlVector = ControlKeys.GetVector(JOYSTICK, Vector3.positiveInfinity);
            if(!tempControlVector.Equals(Vector3.positiveInfinity))
                ParseDir(tempControlVector);
            tempControlVector = ControlKeys.GetVector(SHOOTING_JOYSTICK, Vector3.positiveInfinity);
            if (!tempControlVector.Equals(Vector3.positiveInfinity))
                aimmingPos += tempControlVector * AimmingPosAcce * Time.fixedDeltaTime;
            

            Vector3 curPos = trans.position;
            Quaternion curRot = trans.rotation;

            tempControlVector = ControlKeys.GetVector(STARING_AT, Vector3.positiveInfinity);
            if (tempControlVector.Equals(Vector3.positiveInfinity))
            {
                if (ControlKeys.GetBool(TURN_RIGHT))
                    curRot = TurnRight(curRot);
                if (ControlKeys.GetBool(TURN_LEFT))
                    curRot = Turnleft(curRot);
            }
            else
                curRot = StaringAt(curRot, tempControlVector, curPos);
            trans.rotation = curRot;

            Vector3 curDir = (curRot * Vector3.up).normalized;
            Vector3 curVelocity = rb2d.velocity;

            curVelocity -= drag * Time.fixedDeltaTime * rb2d.velocity.magnitude * curVelocity.normalized;

            if (ControlKeys.GetBool(ACC_FRONT))
                curVelocity = SpeedUpFront(curVelocity, curDir);

            if (ControlKeys.GetBool(ACC_BACK))
                curVelocity = SpeedUpBack(curVelocity, curDir);

            if (ControlKeys.GetBool(ACC_LEFT))
                curVelocity = SpeedUpLeft(curVelocity, curDir);

            if (ControlKeys.GetBool(ACC_RIGHT))
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
            ControlKeys.SetBool(ACC_FRONT,true);
        }
        else if (angle <= TranslationAngle && !ForceTurn)
        {
            Vector3 temp = Vector3.Cross(baseDir, joystickDir);
            if (temp.z < 0)
                ControlKeys.SetBool(ACC_RIGHT, true);
            else
                ControlKeys.SetBool(ACC_LEFT, true);
        }
        else if (angle <= TurnAngle || ForceTurn)
        {
            Vector3 temp = Vector3.Cross(baseDir, joystickDir);
            if (temp.z < 0)
                ControlKeys.SetBool(TURN_RIGHT, true);
            else
                ControlKeys.SetBool(TURN_LEFT, true);
        }
        else
            ControlKeys.SetBool(ACC_BACK, true);
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

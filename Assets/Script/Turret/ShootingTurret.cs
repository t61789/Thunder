using System.Collections;
using UnityEngine;

public class ShootingTurret : Turret
{
    public string Bullet;
    public bool Controllable;
    public float TurnDegree;

    protected Transform shipTrans;
    protected float fireInterval;
    protected float fireIntervalCount;

    protected bool FireControl { get; set; } = false;
    protected Vector3 TargetPosition { get; set; } = Vector3.positiveInfinity;

    protected void Awake()
    {
        trans = transform;
        fireIntervalCount = Time.time;
        SetBullet(Bullet);
    }

    protected void Update()
    {
        if (Controllable)
        {
            if ((Time.time - fireIntervalCount > fireInterval) && FireControl)
            {
                fireIntervalCount = Time.time;
                PublicVar.objectPool.DefaultAlloc<NormalBullet>(Bullet, x=> {
                    x.ObjectPoolInit(shipTrans,trans.position,trans.rotation);
                });
            }
        }
    }

    protected void FixedUpdate()
    {
        if (Controllable)
        {
            if (!TargetPosition.Equals(Vector3.positiveInfinity))
            {
                Vector3 vector = TargetPosition - trans.position;
                trans.rotation = Quaternion.RotateTowards(trans.rotation, Quaternion.FromToRotation(Vector3.up, vector), TurnDegree);
            }
        }
    }

    public override void Install(Ship ship, Vector3 position, Vector3 rotaion)
    {
        base.Install(ship, position, rotaion);
        shipTrans = ship.transform;
    }

    public void SetBullet(string bullet)
    {
        fireInterval = PublicVar.objectPool.GetPrefab(bullet).GetComponent<Bullet>().FireInterval;
        Bullet = bullet;
    }
}

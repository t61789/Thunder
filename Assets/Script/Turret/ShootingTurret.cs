using UnityEngine;

public class ShootingTurret : Turret
{
    public string Bullet;
    public bool Controllable;
    public float TurnDegree;

    protected Transform shipTrans;
    protected float fireInterval;
    protected float fireIntervalCount;

    private const string FIRE = "Fire";

    protected override void Awake()
    {
        base.Awake();
        trans = transform;
        fireIntervalCount = Time.time;
        SetBullet(Bullet);
    }

    protected void Update()
    {
        if (Controllable)
        {
            if ((Time.time - fireIntervalCount > fireInterval) && ControlKeys.GetBool(FIRE))
            {
                fireIntervalCount = Time.time;
                PublicVar.objectPool.Alloc<NormalBullet>(Bullet, x =>
                {
                    x.ObjectPoolInit(shipTrans, trans.position, trans.rotation);
                });
            }
        }
    }

    protected void FixedUpdate()
    {
        if (Controllable)
        {
            Vector3 vector = ship.aimmingPos - trans.position;
            trans.rotation = Quaternion.RotateTowards(trans.rotation, Quaternion.FromToRotation(Vector3.up, vector), TurnDegree);
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

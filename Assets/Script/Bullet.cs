using Tool.BuffData;
using Tool.ObjectPool;
using UnityEngine;

public abstract class Bullet : MonoBehaviour, IObjectPool
{
    public float MaxSpeed;
    public float LifeTime;
    public float Damage;
    public float FireInterval;

    protected Quaternion direction;
    protected BuffData damage;
    protected float lifeTimeStart = 0;
    protected Rigidbody2D rb2d;
    protected ObjectPool objectPool;
    protected Animator animator;
    protected Transform releaserTrans;
    protected Transform trans;
    protected Aircraft releaser;

    protected void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        trans = transform;
        damage = Damage;
    }

    protected void LateUpdate()
    {
        if (Time.time - lifeTimeStart >= LifeTime)
            LifeTimeEnd();
    }

    public virtual void ObjectPoolDestroy()
    {
        Destroy(gameObject);
    }

    public void SetObjectPool(ObjectPool objectPool)
    {
        this.objectPool = objectPool;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public virtual void ObjectPoolReset()
    {
        lifeTimeStart = Time.time;
    }

    public virtual void ObjectPoolRecycle()
    {

    }

    public virtual void LifeTimeEnd()
    {
        PublicVar.objectPool.Recycle(this);
    }

    public virtual void ObjectPoolInit(Transform releaser, Vector3 pos, Quaternion rotate)
    {
        releaserTrans = releaser;
        this.releaser = releaser.GetComponent<Aircraft>();

        trans.rotation = rotate;
        trans.position = pos;

        animator.Play("idle");
    }
}


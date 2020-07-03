using Thunder.Sys;
using Thunder.Tool.BuffData;
using Thunder.Tool.ObjectPool;
using Thunder.Turret;
using UnityEngine;

namespace Thunder
{
    public abstract class Bullet : MonoBehaviour, IObjectPool
    {
        public float MaxSpeed;
        public float LifeTime;
        public float Damage;
        public float FireInterval;

        protected Quaternion direction;
        protected BuffData damage;
        protected float lifeTimeStart;
        protected Rigidbody2D rb2d;
        protected ObjectPool objectPool;
        protected Animator animator;
        protected Transform releaserTrans;
        protected Transform trans;
        protected Aircraft releaser;
        public AssetId AssetId { get; set; }


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

        public virtual void AfterOpDestroy()
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

        public virtual void BeforeOpReset()
        {
            lifeTimeStart = Time.time;
        }

        public virtual void BeforeOpRecycle()
        {

        }

        public virtual void LifeTimeEnd()
        {
            Sys.Stable.objectPool.Recycle(this);
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
}


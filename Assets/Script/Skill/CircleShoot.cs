﻿namespace Thunder.Skill
{
    //public class CircleShoot : Skill
    //{
    //    public int[] BulletNum;
    //    public float FireInterval;

    //    private float skillCdCount;

    //    public struct Values
    //    {
    //        public float skillCd;
    //        public int[] bulletNum;
    //        public float fireInterval;
    //    }

    //    private void Awake()
    //    {
    //        skillCdCount = 0;

    //        Values values = Sys.ValueSys.Ins.GetValue<Values>("skill" + Paths.Div + "circleShoot");

    //        SkillCd = values.skillCd;
    //        BulletNum = values.bulletNum;
    //        FireInterval = values.fireInterval;
    //    }

    //    public override void SkillInit(Hashtable arg)
    //    {
    //    }

    //    public override void SkillRemove()
    //    {
    //        StopAllCoroutines();
    //    }

    //    protected virtual void Update()
    //    {
    //        if (Time.time - skillCdCount >= SkillCd && Sys.ControlSys.Ins.RequestDown(KeyCode.Space, "playerControl"))
    //        {
    //            StartCoroutine(Shoot());
    //            skillCdCount = Time.time;
    //        }
    //    }

    //    private IEnumerator Shoot()
    //    {
    //        float fireInterValStart = Time.time;

    //        for (int i = 0; i < BulletNum.Length; i++)
    //        {
    //            //if (BulletNum[i] != 0)
    //            //{
    //            //    float angle = 360 / BulletNum[i];

    //            //    for (int j = 0; j < BulletNum[i]; j++)
    //            //    {
    //            //        Bullet b = PublicVar.objectPool.Alloc("playerBullet", new Hashtable() {
    //            //            {"direction",Quaternion.AngleAxis(j*angle,Vector3.forward) },
    //            //            { "startPosition",transform.position}
    //            //        }, transform) as Bullet;
    //            //        b.transform.SetParent(PublicVar.container);
    //            //    }
    //            //}

    //            while (Time.time - fireInterValStart < FireInterval)
    //            {
    //                yield return null;
    //            }
    //            fireInterValStart = Time.time;
    //        }
    //    }
    //}
}
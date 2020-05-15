﻿using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using UnityEngine;

public class NormalFighter : Fighter
{
    public string Bullet;
    public Vector3 releaseBulletPos;

    protected bool Shooting { get; set; }
    protected float fireInterval;
    protected float fireIntervalCount;

    protected override void Awake()
    {
        base.Awake();
        if (Bullet != null)
            SetBullet(Bullet);
    }

    protected void Update()
    {
        if (Controllable)
        {
            if ((Time.time - fireIntervalCount > fireInterval) && Shooting)
            {
                fireIntervalCount = Time.time;
                PublicVar.objectPool.Alloc<NormalBullet>(Bullet, x=> {
                    x.ObjectPoolInit(trans,trans.position,trans.rotation,rb2d);
                });
            }
        }
    }

    public void SetBullet(string bullet)
    {
        fireInterval = PublicVar.objectPool.GetPrefab(bullet).GetComponent<Bullet>().FireInterval;
        Bullet = bullet;
        fireIntervalCount = Time.time;
    }

    public TaskStatus StopShoot()
    {
        Shooting = false;
        return TaskStatus.Success;
    }
    public TaskStatus Shoot()
    {
        Shooting = true;
        return TaskStatus.Success;
    }
}

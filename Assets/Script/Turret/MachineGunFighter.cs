using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class MachineGunFighter : Fighter
{
    protected ParticleSystem machineGun;

    protected override void Awake()
    {
        base.Awake();
        machineGun = trans.Find("machineGun").GetComponent<ParticleSystem>();
    }

    public TaskStatus StopShoot()
    {
        machineGun.Stop();
        return TaskStatus.Success;
    }
    public TaskStatus Shoot()
    {
        machineGun.Play();
        return TaskStatus.Success;
    }
}

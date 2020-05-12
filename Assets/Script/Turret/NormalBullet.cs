using System.Collections;
using UnityEngine;

/// <summary>
/// releaser应有rigidbody2d和继承了aircraft的控制器
/// </summary>
public class NormalBullet : Bullet
{
    [Range(0, 1)]
    public float InheritVelocity;

    protected Rigidbody2D releaserRb2d;

    public void ObjectPoolInit(Transform releaser, Vector3 pos, Quaternion rotate,Rigidbody2D releaserRb2d)
    {
        base.ObjectPoolInit(releaser, pos, rotate);
        this.releaserRb2d = releaserRb2d;
        rb2d.velocity = trans.rotation * Vector3.up * MaxSpeed + (Vector3)releaserRb2d.velocity * InheritVelocity;
        animator.Play("idle");
    }

    protected void BulletExploded()
    {
        animator.SetBool("contactEnemy", false);
        PublicVar.objectPool.Recycle(this);
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger)
            return;

        Aircraft camp = other.gameObject.GetComponent<Aircraft>();
        if (camp == null)
            return;
        if (!PublicVar.campManager.IsHostile(releaser, camp))
            return;

        (camp as Aircraft).GetDamage(Damage);

        rb2d.velocity = Vector2.zero;

        animator.SetBool("contactEnemy", true);
    }
}

using BehaviorDesigner.Runtime.Tasks;
using Framework;
using UnityEditor.UIElements;
using UnityEngine;

namespace Thunder
{
    public class Enemy : BaseEntity,IHitAble
    {
        public bool ShowGizmos = false;
        public float MeleeInterval = 1;
        public float MeleeSectorRadius = 1;
        public float MeleeSectorAngle = 45;
        public float MeleeSectorHeight = 0.2f;
        public float MeleeDamage = 20;

        public Health Health;

        private SimpleCounter _FireCounter;

        protected override void Awake()
        {
            base.Awake();
            _FireCounter = new SimpleCounter(MeleeInterval).Complete();
            Health.Init();
            Health.OnDead += Dead;
        }

        private void OnDrawGizmosSelected()
        {
            if (!ShowGizmos) return;

            Gizmos.color = Color.white;
            var trans = transform;
            var forward = trans.forward.ProjectToXz().normalized;
            Gizmos.DrawWireMesh(
                GizmosMesh.GetCircleMesh(MeleeSectorRadius, 20, Vector3.up),
                trans.position);

            Gizmos.color = Color.red;
            var meleeBorder = Quaternion.AngleAxis(MeleeSectorAngle / 2, Vector3.up) * forward;
            Gizmos.DrawLine(trans.position, meleeBorder * MeleeSectorRadius + trans.position);
            meleeBorder = Quaternion.AngleAxis(-MeleeSectorAngle / 2, Vector3.up) * forward;
            Gizmos.DrawLine(trans.position, meleeBorder * MeleeSectorRadius + trans.position);
            Gizmos.DrawLine(forward * MeleeSectorRadius + MeleeSectorHeight * Vector3.up / 2 + trans.position,
                forward * MeleeSectorRadius + MeleeSectorHeight * Vector3.down / 2 + trans.position);
        }

        public TaskStatus MeleeHit()
        {
            if (_FireCounter.Completed)
            {
                _FireCounter.Recount();

                var forward = Trans.forward.ProjectToXz();
                foreach (var raycastHit in Physics.SphereCastAll(Trans.position, MeleeSectorRadius, Vector3.one))
                {
                    if(raycastHit.transform==Trans)continue;
                    var pos = raycastHit.transform.position;
                    if (Mathf.Abs((pos - Trans.position).y) > MeleeSectorHeight)
                        continue;
                    if (Vector3.Angle(forward, pos- Trans.position) > MeleeSectorAngle / 2)
                        continue;
                    raycastHit.transform.gameObject.GetComponent<IHitAble>()?.GetHit(
                        pos,
                        (pos - Trans.position).normalized,
                        MeleeDamage);
                }
            }

            return TaskStatus.Success;
        }

        public TaskStatus PlayerInAttackRange()
        {
            if (Player.Ins == null) return TaskStatus.Failure;
            if ((Player.Ins.Trans.position - Trans.position).magnitude <= MeleeSectorRadius * 0.7f)
                return TaskStatus.Success;
            return TaskStatus.Failure;
        }

        public void GetHit(Vector3 hitPos, Vector3 hitDir, float damage)
        {
            Health.CostHealth(damage);
        }

        private void Dead()
        {
            Destroy(gameObject);
        }
    }
}

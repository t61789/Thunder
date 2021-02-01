using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Framework;
using UnityEditor.UIElements;
using UnityEngine;
using Action = System.Action;

namespace Thunder
{
    public class Enemy : BaseCharacter,IObjectPool
    {
        public bool ShowGizmos = false;
        public float MeleeInterval = 1;
        public float MeleeSectorRadius = 1;
        public float MeleeSectorAngle = 45;
        public float MeleeSectorHeight = 0.2f;
        public float MeleeRangeBufferFactor = 0.9f;
        public float MeleeDamage = 20;
        public float PursueRange = 3;

        [HideInInspector] public BaseCharacter CurTarget;
        private SimpleCounter _FireCounter;
        private readonly Dictionary<BaseCharacter,Action> _Targets
            = new Dictionary<BaseCharacter, Action>();
        private Animator _Animator;

        protected override void Awake()
        {
            base.Awake();
            _FireCounter = new SimpleCounter(MeleeInterval).Complete();
            _Animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            if (CurTarget != null && 
                (CurTarget.Trans.position - Trans.position).sqrMagnitude > PursueRange * PursueRange)
                UnBindTarget(CurTarget);
        }

        private void OnTriggerEnter(Collider col)
        {
            if (!col.GetComponent<BaseCharacter>(out var character)) return;
            if (CampSys.IsAlly(Camp,character.Camp)) return;

            BindTarget(character);
        }

        private void OnTriggerExit(Collider col)
        {
            if (!col.GetComponent<BaseCharacter>(out var character)) return;
            
            if(character!=CurTarget)
                UnBindTarget(character);
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
            //if (_FireCounter.Completed)
            //{
            //    _FireCounter.Recount();

            //    var forward = Trans.forward.ProjectToXz();
            //    foreach (var raycastHit in Physics.SphereCastAll(Trans.position, MeleeSectorRadius, Vector3.one))
            //    {
            //        if (raycastHit.transform == Trans) continue;
            //        var pos = raycastHit.transform.position;
            //        if (Mathf.Abs((pos - Trans.position).y) > MeleeSectorHeight)
            //            continue;
            //        if (Vector3.Angle(forward, pos - Trans.position) > MeleeSectorAngle / 2)
            //            continue;
            //        raycastHit.transform.gameObject.GetComponent<IHitAble>()?.GetHit(
            //            pos,
            //            (pos - Trans.position).normalized,
            //            MeleeDamage);
            //    }
            //}

            var target = GetTarget();
            if (target == null || !_FireCounter.Completed)
                return TaskStatus.Failure;

            _FireCounter.Recount();

            target.GetHit(
                target.Trans.position,
                (target.Trans.position - Trans.position).normalized,
                MeleeDamage);

            _Animator.SetTrigger("Hit");
            return TaskStatus.Success;
        }

        public TaskStatus TargetInHitRange()
        {
            var target = GetTarget();

            if (target == null ||
                (target.Trans.position - Trans.position).sqrMagnitude > MeleeSectorRadius)
            {
                //Debug.Log(1);
                return TaskStatus.Failure;
            }

            //Debug.Log(2);

            return TaskStatus.Success;
        }

        public TaskStatus SwitchAnimatorMove()
        {
            _Animator.SetBool("Walk",!_Animator.GetBool("Walk"));
            return TaskStatus.Success;
        }

        public BaseCharacter GetTarget()
        {
            if (CurTarget != null) return CurTarget;

            CurTarget = null;
            float min = float.MaxValue;
            foreach (var o in _Targets.Keys)
            {
                var distance = (o.Trans.position - Trans.position).sqrMagnitude;
                if (distance < min && distance < MeleeSectorRadius * MeleeRangeBufferFactor)
                {
                    CurTarget = o;
                    min = distance;
                }
            }

            return CurTarget;
        }

        public override void GetHit(Vector3 hitPos, Vector3 hitDir, float damage)
        {
            Health.Cost(damage);
        }

        protected override void Dead()
        {
            base.Dead();
            foreach (var target in _Targets)
                target.Key.Health.ReachMin -= target.Value;

            GameObjectPool.Put(this);
        }

        private void TargetDead(BaseCharacter character)
        {
            UnBindTarget(character);
        }

        private void BindTarget(BaseCharacter character)
        {
            if (_Targets.ContainsKey(character)) return;

            void DeadAction() => TargetDead(character);
            character.Health.ReachMin += DeadAction;
            _Targets.Add(character, DeadAction);
        }

        private void UnBindTarget(BaseCharacter character)
        {
            if (!_Targets.TryGetValue(character, out var act)) return;
            if (character == CurTarget) CurTarget = null;
            character.Health.ReachMin -= act;
            _Targets.Remove(character);
        }

        public AssetId AssetId { get; set; }

        public void OpReset()
        {
            _FireCounter.Complete();
            Health.Cur = Health.Max;
        }

        public void OpPut() { }

        public void OpDestroy() { }
    }
}

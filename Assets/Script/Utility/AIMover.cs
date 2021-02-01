using Framework;
using UnityEngine;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Thunder
{
    [RequireComponent(typeof(Enemy))]
    public class AIMover : Mover
    {
        private Vector3 _MoveDir;
        private bool _Jump;
        private bool _Squat;
        private Enemy _Enemy;

        protected override void Awake()
        {
            base.Awake();
            _Enemy = GetComponent<Enemy>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            var moveTarget = GetMoveTargetTrans();
            var dir = (moveTarget.position - Trans.position).ProjectToXz().normalized;
            Trans.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        public void Clear()
        {
            _MoveDir = Vector3.zero;
            _Jump = false;
            _Squat = false;
        }

        public void Move(Vector3 dir)
        {
            _MoveDir = dir;
        }

        public TaskStatus Move()
        {
            var moveTarget = GetMoveTargetTrans();
            var pos = moveTarget == null ? Trans.position : moveTarget.position;
            var dir = (pos - Trans.position).ProjectToXz().normalized;
            Move(dir);
            return TaskStatus.Success;
        }

        public TaskStatus Jump()
        {
            _Jump = true;
            return TaskStatus.Success;
        }

        public TaskStatus Squat()
        {
            _Squat = true;
            return TaskStatus.Success;
        }

        public Transform GetMoveTargetTrans()
        {
            var target = _Enemy.GetTarget();
            return target == null ? GlobalResource.Ins.GetFinalTarget() : target.Trans;
        }

        protected override ControlInfo GetMoveControl()
        {
            var result = new ControlInfo(_MoveDir, _MoveDir!=Vector3.zero, false, false);
            _MoveDir = Vector3.zero;
            return result;
        }

        protected override ControlInfo GetJumpControl()
        {
            var result = new ControlInfo(Vector3.zero, _Jump, _Jump, false);
            _Jump = false;
            return result;
        }

        protected override ControlInfo GetSquatControl()
        {
            var result = new ControlInfo(Vector3.zero, _Squat, _Squat, false);
            _Squat = false;
            return result;
        }
    }
}

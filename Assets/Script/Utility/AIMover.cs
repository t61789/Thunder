using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Thunder
{
    public class AIMover : Mover
    {
        private Vector3 _MoveDir;
        private bool _Jump;
        private bool _Squat;

        public void Move(Vector3 dir)
        {
            _MoveDir = dir;
        }

        public TaskStatus MoveTowardsPlayer()
        {
            if (Player.Ins == null) return TaskStatus.Success;
            var dir = (Player.Ins.Trans.position - Trans.position).ProjectToXz().normalized;
            Move(dir);
            Trans.rotation = Quaternion.LookRotation(dir, Vector3.up);

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

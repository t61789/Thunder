using System;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class FpsMover : Mover
    {
        public string MoveKey = "fps_mover_move_key";
        public string JumpKey = "fps_mover_jump_key";
        public string SquatKey = "fps_mover_squat_key";

        protected override ControlInfo GetMoveControl()
        {
            var ctrl = ControlSys.RequireKey(CtrlKeys.GetKey(MoveKey));
            ctrl.HandleRawAxis();
            ctrl.Axis = Trans.localToWorldMatrix * ctrl.Axis;

            return ctrl;
        }

        protected override ControlInfo GetJumpControl()
        {
            return ControlSys.RequireKey(CtrlKeys.GetKey(JumpKey));
        }

        protected override ControlInfo GetSquatControl()
        {
            return ControlSys.RequireKey(CtrlKeys.GetKey(SquatKey));
        }
    }
}

using System;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class FpsMover : Mover
    {
        public CtrlKey MoveKey;
        public CtrlKey JumpKey;
        public CtrlKey SquatKey;

        protected override ControlInfo GetMoveControl()
        {
            var ctrl = ControlSys.RequireKey(MoveKey);
            ctrl.HandleRawAxis();
            ctrl.Axis = Trans.localToWorldMatrix * ctrl.Axis;

            return ctrl;
        }

        protected override ControlInfo GetJumpControl()
        {
            return ControlSys.RequireKey(JumpKey);
        }

        protected override ControlInfo GetSquatControl()
        {
            return ControlSys.RequireKey(SquatKey);
        }
    }
}

using Thunder.Sys;
using UnityEngine;

namespace Thunder.Utility
{
    [RequireComponent(typeof(Camera))]
    public class FreeLookCamera : BaseCamera
    {
        public Transform Target;
        public float Sensitive;
        public float FollowRange;
        public float MaxFollowRange;
        public float LimitAngle;
        public Vector3 TargetOffset;
        [Range(0, 1)]
        public float SmoothFactor;

        private Vector3 _TargetRot;
        private Vector3 _PreTargetPos;
        private float _TargetFollowRange;
        private Quaternion _FixedRot;
        private Vector3 _FixedPos;

        private void OnEnable()
        {
            _TargetFollowRange = FollowRange;
            SetTarget(Target);
            _FixedPos = Trans.position;
            _FixedRot = Trans.rotation;
        }

        private void FixedUpdate()
        {
            if (Target == null) return;
            Vector3 deltaAxis = ControlSys.Ins.RequireKey("Axis2", 0).Axis * Sensitive;

            _TargetRot.y += deltaAxis.x;
            _TargetRot.x -= deltaAxis.y;

            float tempAngle = LimitAngle / 2;
            _TargetRot.x = Mathf.Clamp(_TargetRot.x, -tempAngle, tempAngle);

            _FixedRot = Quaternion.Lerp(_FixedRot, Quaternion.Euler(_TargetRot), SmoothFactor);

            float scrollDelta = ControlSys.Ins.RequireKey("Axis3", 0).Axis.x * Sensitive;
            _TargetFollowRange = Mathf.Clamp(_TargetFollowRange - scrollDelta, 0, MaxFollowRange);
            FollowRange = Mathf.Lerp(FollowRange, _TargetFollowRange, SmoothFactor);

            deltaAxis = _FixedRot * Vector3.back * FollowRange;
            _PreTargetPos = Vector3.Lerp(_PreTargetPos, Target.position + TargetOffset, SmoothFactor);
            _FixedPos = _PreTargetPos + deltaAxis;
        }

        private void LateUpdate()
        {
            Trans.position = Vector3.Lerp(Trans.position, _FixedPos, SmoothFactor);
            Trans.localRotation = Quaternion.Lerp(Trans.localRotation, _FixedRot, SmoothFactor);
        }

        private void SetTarget(Transform target)
        {
            Target = target;
        }
    }
}

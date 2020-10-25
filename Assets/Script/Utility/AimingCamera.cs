using Tool;

using UnityEngine;

namespace Thunder.Utility
{
    public class AimingCamera : BaseCamera
    {
        private Vector3 _TargetDir;
        private Vector3 _TargetPos;
        private Vector3 _TargetRot;
        public float OffsetAngle;
        public float Radius;

        public bool RealTimeUpdate;
        public float Sensitive;

        [Range(0, 1)] public float SmoothFactor;

        public Transform Target;
        public Vector3 TargetOffset;
        public float UpAngleLimit;

        private void OnEnable()
        {
            _TargetDir = Trans.position;
            SetTarget(Target);
        }

        private void FixedUpdate()
        {
            //if (Target == null) return;
            //Vector3 targetPos = Target.position + TargetOffset;
            //Vector3 dir = (_TargetDir - targetPos).normalized * Radius;
            //Vector3 input = ControlSys.Ins.RequireKey("Axis2", 0).Axis;
            //dir = Quaternion.AngleAxis(Sensitive * input.x, Vector3.up)*dir;
            //_TargetDir = dir + targetPos;
            //float preEulerx = Trans.localEulerAngles.x;
            //if (preEulerx >= 270) preEulerx -= 360;

            //dir = Vector3.Cross(Vector3.down, dir);
            //dir = Quaternion.AngleAxis(OffsetAngle, Vector3.up) * dir;

            //Trans.rotation = Quaternion.LookRotation(dir, Vector3.up);
            //// Trans.localEulerAngles += input.y * Sensitive * Vector3.right;

            //float angle = input.y * -Sensitive + preEulerx;
            //angle = Mathf.Clamp(angle, -UpAngleLimit, UpAngleLimit);
            //dir = Trans.localEulerAngles;
            //dir.x = angle;
            //Trans.localEulerAngles = dir;
            //Debug.Log(Trans.localEulerAngles);

            //// y
            //if (Target == null) return;
            //Vector3 targetPos = Target.position + TargetOffset;
            //Vector3 dir = (_TargetDir - targetPos).normalized * Radius;
            //Vector3 input = ControlSys.Ins.RequireKey("Axis2", 0).Axis;
            //float yRotAngle = Sensitive * input.x;
            //Quaternion targetRot = Quaternion.AngleAxis(yRotAngle, Vector3.up);
            //dir = targetRot * dir;
            //_TargetDir = dir + targetPos;
            //Trans.localRotation *= targetRot;

            //// x
            //float preEulerx = Trans.localEulerAngles.x;
            //if (preEulerx >= 270) preEulerx -= 360;
            //float angle = input.y * -Sensitive + preEulerx;
            //angle = Mathf.Clamp(angle, -UpAngleLimit, UpAngleLimit);
            //dir = Trans.localEulerAngles;
            //dir.x = angle;
            //Trans.localEulerAngles = dir;

            //if (Target == null) return;
            //Vector3 input = ControlSys.Ins.RequireKey("Axis2", 0).Axis;
            //float yRotAngle = Sensitive * input.x;
            //_TargetDir = Quaternion.AngleAxis(yRotAngle, Vector3.up) * _TargetDir;
            //Vector3 eulerAngle = Trans.eulerAngles;
            //eulerAngle += Vector3.up * yRotAngle;
            //eulerAngle.y %= 360;

            //if (eulerAngle.x >= 270) eulerAngle.x -= 360;
            //eulerAngle.x = Mathf.Clamp(input.y * -Sensitive + eulerAngle.x, -UpAngleLimit, UpAngleLimit);
            //Trans.eulerAngles = eulerAngle;

            //if (Target == null) return;
            //Vector3 input = ControlSys.Ins.RequireKey("Axis2", 0).Axis;
            //float yRotAngle = Sensitive * input.x;
            //_TargetDir = Quaternion.AngleAxis(yRotAngle, Vector3.up) * _TargetDir;
            //Vector3 dir = Vector3.Cross(_TargetDir, Vector3.up);
            //dir = Quaternion.AngleAxis(OffsetAngle, Vector3.up) * dir;

            //_TargetRot.y = Vector3.SignedAngle(Vector3.forward, dir,Vector3.up);
            //if (_TargetRot.x >= 270) _TargetRot.x -= 360;
            //_TargetRot.x = Mathf.Clamp(input.y * -Sensitive + _TargetRot.x, -UpAngleLimit, UpAngleLimit);
            //_TargetRot.z = 0;

            if (Target == null) return;
            var input = ControlSys.Ins.RequireKey("Axis2", 0).Axis;
            var yRotAngle = Sensitive * input.x;
            _TargetDir = Quaternion.AngleAxis(yRotAngle, Vector3.up) * _TargetDir;
            var dir = Vector3.Cross(_TargetDir, Vector3.up);
            dir = Quaternion.AngleAxis(OffsetAngle, Vector3.up) * dir;

            _TargetRot.y = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
            if (_TargetRot.x >= 270) _TargetRot.x -= 360;
            _TargetRot.x = Mathf.Clamp(input.y * -Sensitive + _TargetRot.x, -UpAngleLimit, UpAngleLimit);
            _TargetRot.z = 0;

            _TargetPos = Target.position;
        }

        public void SetTarget(Transform trans)
        {
            Target = trans;
            if (trans == null) return;
            //_TargetDir = (Trans.position - Target.position - TargetOffset).ProjectToxz().normalized * Radius;
            //Vector3 dir = Vector3.Cross(_TargetDir, Vector3.up);
            //dir = Quaternion.AngleAxis(OffsetAngle, Vector3.up) * dir;
            //Trans.rotation = Quaternion.LookRotation(dir, Vector3.up);
            _TargetDir = (Trans.position - Target.position - TargetOffset).ProjectToxz().normalized;
            _TargetRot = Trans.eulerAngles;
        }

        private void LateUpdate()
        {
            //if (Target == null) return;

            Trans.position = Vector3.Lerp(Trans.position, _TargetDir * Radius + Target.position + TargetOffset,
                SmoothFactor);
            //Trans.position = _TargetDir * Radius + Target.position + TargetOffset;
            //Trans.rotation = Quaternion.Lerp(Trans.rotation,Quaternion.Euler(_TargetRot),SmoothFactor);
            Trans.rotation = Quaternion.Euler(_TargetRot);
        }
    }
}
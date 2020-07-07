using System;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityVector2;
using Thunder.Sys;
using Thunder.Turret;
using UnityEngine;

namespace Thunder
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        //public Ship FollowTarget
        //{
        //    get => followTarget;
        //    set
        //    {
        //        if (value != null)
        //            curCamera.orthographicSize = value.GetComponent<SpriteRenderer>().bounds.size.y * SizeCoe;
        //        followTarget = value;
        //    }
        //}
        //public bool OffsetFollow;
        //public float OffsetLerp;
        //public float MoveDampTime;
        //public float SizeCoe = 1;

        //private Vector3 targetPrePosition = Vector2.zero;
        //private Ship followTarget;
        //private Camera curCamera;

        //private void Awake()
        //{
        //    curCamera = GetComponent<Camera>();
        //}

        //public void LateUpdate()
        //{
        //    if (FollowTarget == null) return;
        //    if (OffsetFollow)
        //    {
        //        Vector3 tempPosition = transform.position;
        //        tempPosition += FollowTarget.trans.position - targetPrePosition;

        //        Vector3 newPosition = Vector2.Lerp(FollowTarget.trans.position, FollowTarget.aimmingPos, OffsetLerp);

        //        Vector3 temp = new Vector3();
        //        tempPosition = Vector3.SmoothDamp(tempPosition, newPosition, ref temp, MoveDampTime);
        //        tempPosition.z = -10;
        //        transform.position = tempPosition;

        //        targetPrePosition = FollowTarget.trans.position;
        //    }
        //    else
        //    {
        //        transform.position = FollowTarget.trans.position + Vector3.forward * (-10);
        //    }
        //}

        public FreeLookCamera.Param FreeLookParam;

        public Transform Target;

        [SerializeField]
        private CameraMode _CameraMode;

        private BaseCamera _CurCamera;

        private void Awake()
        {
            SwitchCameraMode(_CameraMode);
        }
        private void Update()
        {
            _CurCamera?.Update();
        }
        private void FixedUpdate()
        {
            _CurCamera?.FixedUpdate();
        }
        private void LateUpdate()
        {
            _CurCamera?.LateUpdate();
        }

        public void SwitchCameraMode(CameraMode mode)
        {
            if (mode == CameraMode.FreeLook)
            {
                _CurCamera = new FreeLookCamera(FreeLookParam,transform);
                _CurCamera.SetTarget(Target);
            }
        }
    }

    public abstract class BaseCamera
    {
        public Transform Target;
        protected Transform _Trans;
        protected BaseCamera(Transform trans)
        {
            _Trans = trans;
        }

        public void SetTarget(Transform target)
        {
            Target = target;
        }

        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void LateUpdate();
    }

    public class FreeLookCamera : BaseCamera
    {
        private Vector3 _TargetRot;
        private Vector3 _PreTargetPos;
        private float _TargetFollowRange;

        [Serializable]
        public struct Param
        {
            public float Sensitive;
            public float FollowRange;
            public float MaxFollowRange;
            public float LimitAngle;
            public Vector3 TargetOffset;
            [Range(0, 1)]
            public float SmoothFactor;
        }

        private Param _Param;

        public FreeLookCamera(Param param,Transform trans) : base(trans)
        {
            _Param = param;
            _TargetFollowRange = param.FollowRange;
        }

        public override void Update()
        {
            
        }

        public override void FixedUpdate()
        {
            
        }

        public override void LateUpdate()
        {
            if (Target == null) return;
            Vector3 deltaAxis = Stable.Control.RequireKey("Axis2", 0).Axis * _Param.Sensitive;

            _TargetRot.y += deltaAxis.x;
            _TargetRot.x -= deltaAxis.y;

            float tempAngle = _Param.LimitAngle / 2;
            _TargetRot.x = Mathf.Clamp(_TargetRot.x, -tempAngle, tempAngle);

            _Trans.localRotation = Quaternion.Lerp(_Trans.localRotation, Quaternion.Euler(_TargetRot), _Param.SmoothFactor);

            float scrollDelta = Stable.Control.RequireKey("Axis3", 0).Axis.x * _Param.Sensitive;
            _TargetFollowRange = Mathf.Clamp(_TargetFollowRange - scrollDelta, 0, _Param.MaxFollowRange);
            _Param.FollowRange = Mathf.Lerp(_Param.FollowRange, _TargetFollowRange, _Param.SmoothFactor);

            deltaAxis = _Trans.rotation * Vector3.back * _Param.FollowRange;
            _PreTargetPos = Vector3.Lerp(_PreTargetPos, Target.position + _Param.TargetOffset, _Param.SmoothFactor);
            _Trans.position = _PreTargetPos + deltaAxis;
        }
    }

    public enum CameraMode
    {
        FreeLook
    }
}


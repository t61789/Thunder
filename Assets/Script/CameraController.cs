using System;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityVector2;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.Turret;
using UnityEngine;

namespace Thunder
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public FreeLookCamera.ParamStruct FreeLookParam;
        public PlayerLockCamera.ParamStruct PlayerLockParam;

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
            _CurCamera?.Dispose();
            _CurCamera = null;
            if (mode == CameraMode.FreeLook)
            {
                _CurCamera = new FreeLookCamera(transform,this);
            }else if (mode == CameraMode.PlayerLock)
            {
                _CurCamera = new PlayerLockCamera(transform, this);
            }
        }
    }
    public enum CameraMode
    {
        FreeLook,
        PlayerLock
    }

    public abstract class BaseCamera
    {
        protected Transform _Trans;
        protected CameraController _CameraController;
        protected BaseCamera(Transform trans,CameraController cameraController)
        {
            _Trans = trans;
            _CameraController = cameraController;
        }

        public abstract void Dispose();
        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void LateUpdate();
    }

    public class FreeLookCamera : BaseCamera
    {
        private Vector3 _TargetRot;
        private Vector3 _PreTargetPos;
        private float _TargetFollowRange;
        public Transform Target;

        private Quaternion _FixedRot;
        private Vector3 _FixedPos;

        [Serializable]
        public struct ParamStruct
        {
            public Transform Target;
            public float Sensitive;
            public float FollowRange;
            public float MaxFollowRange;
            public float LimitAngle;
            public Vector3 TargetOffset;
            [Range(0, 1)]
            public float SmoothFactor;

            public ParamStruct(Transform target, float sensitive, float followRange, float maxFollowRange, float limitAngle, Vector3 targetOffset, float smoothFactor)
            {
                Target = target;
                Sensitive = sensitive;
                FollowRange = followRange;
                MaxFollowRange = maxFollowRange;
                LimitAngle = limitAngle;
                TargetOffset = targetOffset;
                SmoothFactor = smoothFactor;
            }
        }

        public ParamStruct Param;

        public FreeLookCamera(Transform trans,CameraController controller) : base(trans, controller)
        {
            Param = controller.FreeLookParam;
            _TargetFollowRange = Param.FollowRange;
            SetTarget(Param.Target);
            _FixedPos = trans.position;
            _FixedRot = trans.rotation;
        }

        public override void Dispose()
        {
            _CameraController.FreeLookParam = Param;
        }

        public override void Update()
        {
            
        }

        public override void FixedUpdate()
        {
            if (Target == null) return;
            Vector3 deltaAxis = Stable.Control.RequireKey("Axis2", 0).Axis * Param.Sensitive;

            _TargetRot.y += deltaAxis.x;
            _TargetRot.x -= deltaAxis.y;

            float tempAngle = Param.LimitAngle / 2;
            _TargetRot.x = Mathf.Clamp(_TargetRot.x, -tempAngle, tempAngle);

            _FixedRot = Quaternion.Lerp(_FixedRot, Quaternion.Euler(_TargetRot), Param.SmoothFactor);

            float scrollDelta = Stable.Control.RequireKey("Axis3", 0).Axis.x * Param.Sensitive;
            _TargetFollowRange = Mathf.Clamp(_TargetFollowRange - scrollDelta, 0, Param.MaxFollowRange);
            Param.FollowRange = Mathf.Lerp(Param.FollowRange, _TargetFollowRange, Param.SmoothFactor);

            deltaAxis = _FixedRot * Vector3.back * Param.FollowRange;
            _PreTargetPos = Vector3.Lerp(_PreTargetPos, Target.position + Param.TargetOffset, Param.SmoothFactor);
            _FixedPos = _PreTargetPos + deltaAxis;
        }

        public override void LateUpdate()
        {
            _Trans.position = Vector3.Lerp(_Trans.position, _FixedPos, Param.SmoothFactor);
            _Trans.localRotation = Quaternion.Lerp(_Trans.localRotation, _FixedRot, Param.SmoothFactor);
        }

        public void SetTarget(Transform target)
        {
            Target = target;
        }
    }

    public class PlayerLockCamera : BaseCamera
    {
        private Transform _Player;
        private Transform _Target;

        [Serializable]
        public struct ParamStruct
        {
            public float ScrollSensitive;
            public float MaxShoulderScale;
            public Transform Player;
            public Transform Target;
            public Vector3 ShoulderOffset;
            public Vector3 TargetOffset;
            public Vector3 PlayerOffset;
            [Range(0,1)]
            public float StaringFactor;
            [Range(0,1)]
            public float SmoothFactor;
            public float ShoulderScale;

            public ParamStruct(float scrollSensitive, float maxShoulderScale, Transform player, Transform target, Vector3 shoulderOffset, Vector3 targetOffset, Vector3 playerOffset, float staringFactor, float smoothFactor, float shoulderScale)
            {
                ScrollSensitive = scrollSensitive;
                MaxShoulderScale = maxShoulderScale;
                Player = player;
                Target = target;
                ShoulderOffset = shoulderOffset;
                TargetOffset = targetOffset;
                PlayerOffset = playerOffset;
                StaringFactor = staringFactor;
                SmoothFactor = smoothFactor;
                ShoulderScale = shoulderScale;
            }
        }

        public ParamStruct Param;
        private Quaternion _TargetRot;
        private Vector3 _TargetPos;
        private float _TargetShoulderScale;

        public PlayerLockCamera(Transform trans,CameraController controller) : base(trans, controller)
        {
            Param = controller.PlayerLockParam;
            _TargetRot = trans.rotation;
            _TargetPos = trans.position;
            _TargetShoulderScale = Param.ShoulderScale;
            SetTargetAndPlayer(Param.Player, Param.Target);
        }

        public void SetTargetAndPlayer(Transform player, Transform target)
        {
            _Player = player;
            _Target = target;
        }

        public override void Dispose()
        {
            _CameraController.PlayerLockParam = Param;
        }

        public override void Update()
        {
        }

        public override void FixedUpdate()
        {
            if (_Target == null || _Player == null) return;

            Vector3 playerPos = _Player.position + Param.PlayerOffset;
            Vector3 targetPos = _Target.position + Param.TargetOffset;

            Vector3 p2TDir = targetPos - playerPos;
            Vector3 p2TDirFlat = new Vector3(p2TDir.x, 0, p2TDir.z).normalized;
            Vector3 p2CDir = _Trans.position - playerPos;
            Vector3 p2CDirFlat = new Vector3(p2CDir.x, 0, p2CDir.z).normalized;
            float right = Mathf.Sign(Vector3.Cross(p2CDirFlat, p2TDirFlat).y);
            Vector3 xAxis = Vector3.Cross(p2TDirFlat, Vector3.up * right).normalized;
            p2TDirFlat = -p2TDirFlat;
            Matrix4x4 c2W = new Matrix4x4(
                new Vector4(xAxis.x, 0, xAxis.z, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(p2TDirFlat.x, 0, p2TDirFlat.z, 0),
                new Vector4(playerPos.x, playerPos.y, playerPos.z, 1));
            float scrollDelta = Stable.Control.RequireKey("Axis3", 0).Axis.x * Param.ScrollSensitive;
            _TargetShoulderScale = Mathf.Clamp(_TargetShoulderScale - scrollDelta, 0, Param.MaxShoulderScale);
            Param.ShoulderScale = Mathf.Lerp(Param.ShoulderScale, _TargetShoulderScale, Param.SmoothFactor);

            _TargetPos = (c2W * (Param.ShoulderOffset * Param.ShoulderScale).ToV4Pos()).ToV3Pos();
            _TargetRot = Quaternion.LookRotation(p2TDir * Param.StaringFactor + playerPos - _TargetPos);
        }

        public override void LateUpdate()
        {
            //Vector3 temp = default;
            //_Trans.position =Vector3.SmoothDamp(_Trans.position, _TargetPos, ref temp, Param.SmoothFactor);
            _Trans.position = Vector3.Lerp(_Trans.position, _TargetPos, Param.SmoothFactor);
            _Trans.rotation = Quaternion.Lerp(_Trans.rotation, _TargetRot, Param.SmoothFactor);
        }
    }
}


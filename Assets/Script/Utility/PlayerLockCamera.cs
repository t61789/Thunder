using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Sys;
using Thunder.Tool;
using UnityEngine;

namespace Thunder.Utility
{
    [RequireComponent(typeof(Camera))]
    public class PlayerLockCamera : BaseCamera
    {
        public float ScrollSensitive;
        public float MaxShoulderScale;
        public Transform Player;
        public Transform Target;
        public Vector3 ShoulderOffset;
        public Vector3 TargetOffset;
        public Vector3 PlayerOffset;
        [Range(0, 1)]
        public float StaringFactor;
        [Range(0, 1)]
        public float SmoothFactor;
        public float ShoulderScale;

        private Quaternion _TargetRot;
        private Vector3 _TargetPos;
        private float _TargetShoulderScale;

        private void OnEnable()
        {
            _TargetRot = Trans.rotation;
            _TargetPos = Trans.position;
            _TargetShoulderScale = ShoulderScale;
            SetTargetAndPlayer(Player, Target);
        }

        public void SetTargetAndPlayer(Transform player, Transform target)
        {
            Player = player;
            Target = target;
        }

        private void FixedUpdate()
        {
            if (Target == null || Player == null) return;

            Vector3 playerPos = Player.position + PlayerOffset;
            Vector3 targetPos = Target.position + TargetOffset;

            Vector3 p2TDir = targetPos - playerPos;
            Vector3 p2TDirFlat = new Vector3(p2TDir.x, 0, p2TDir.z).normalized;
            Vector3 p2CDir = Trans.position - playerPos;
            Vector3 p2CDirFlat = new Vector3(p2CDir.x, 0, p2CDir.z).normalized;
            float right = Mathf.Sign(Vector3.Cross(p2CDirFlat, p2TDirFlat).y);
            Vector3 xAxis = Vector3.Cross(p2TDirFlat, Vector3.up * right).normalized;
            p2TDirFlat = -p2TDirFlat;
            Matrix4x4 c2W = new Matrix4x4(
                new Vector4(xAxis.x, 0, xAxis.z, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(p2TDirFlat.x, 0, p2TDirFlat.z, 0),
                new Vector4(playerPos.x, playerPos.y, playerPos.z, 1));
            float scrollDelta = Stable.Control.RequireKey("Axis3", 0).Axis.x * ScrollSensitive;
            _TargetShoulderScale = Mathf.Clamp(_TargetShoulderScale - scrollDelta, 0, MaxShoulderScale);
            ShoulderScale = Mathf.Lerp(ShoulderScale, _TargetShoulderScale, SmoothFactor);

            _TargetPos = (c2W * (ShoulderOffset * ShoulderScale).ToV4Pos()).ToV3Pos();
            _TargetRot = Quaternion.LookRotation(p2TDir * StaringFactor + playerPos - _TargetPos);
        }

        private void LateUpdate()
        {
            //Vector3 temp = default;
            //_Trans.position =Vector3.SmoothDamp(_Trans.position, _TargetPos, ref temp, SmoothFactor);
            Trans.position = Vector3.Lerp(Trans.position, _TargetPos, SmoothFactor);
            Trans.rotation = Quaternion.Lerp(Trans.rotation, _TargetRot, SmoothFactor);
        }
    }
}

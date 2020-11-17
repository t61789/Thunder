

using Framework;
using UnityEngine;

namespace Thunder
{
    [RequireComponent(typeof(Camera))]
    public class PlayerLockCamera : BaseCamera
    {
        private Vector3 _TargetPos;

        private Quaternion _TargetRot;
        private float _TargetShoulderScale;
        public float MaxShoulderScale;
        public Transform Player;
        public Vector3 PlayerOffset;
        public float ScrollSensitive;
        public Vector3 ShoulderOffset;
        public float ShoulderScale;

        [Range(0, 1)] public float SmoothFactor;

        [Range(0, 1)] public float StaringFactor;

        public Transform Target;
        public Vector3 TargetOffset;

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

            var playerPos = Player.position + PlayerOffset;
            var targetPos = Target.position + TargetOffset;

            var p2TDir = targetPos - playerPos;
            var p2TDirFlat = new Vector3(p2TDir.x, 0, p2TDir.z).normalized;
            var p2CDir = Trans.position - playerPos;
            var p2CDirFlat = new Vector3(p2CDir.x, 0, p2CDir.z).normalized;
            var right = Mathf.Sign(Vector3.Cross(p2CDirFlat, p2TDirFlat).y);
            var xAxis = Vector3.Cross(p2TDirFlat, Vector3.up * right).normalized;
            p2TDirFlat = -p2TDirFlat;
            var c2W = new Matrix4x4(
                new Vector4(xAxis.x, 0, xAxis.z, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(p2TDirFlat.x, 0, p2TDirFlat.z, 0),
                new Vector4(playerPos.x, playerPos.y, playerPos.z, 1));
            var scrollDelta = ControlSys.RequireKey("Axis3", 0).Axis.x * ScrollSensitive;
            _TargetShoulderScale = Mathf.Clamp(_TargetShoulderScale - scrollDelta, 0, MaxShoulderScale);
            ShoulderScale = Mathf.Lerp(ShoulderScale, _TargetShoulderScale, SmoothFactor);

            _TargetPos = (c2W * (ShoulderOffset * ShoulderScale).ToV4Pos()).ToV3Pos();
            _TargetRot = Quaternion.LookRotation(p2TDir * StaringFactor + playerPos - _TargetPos);
        }

        private void LateUpdate()
        {
            //Vector3 temp = default;
            //Trans.position =Vector3.SmoothDamp(Trans.position, _TargetPos, ref temp, SmoothFactor);
            Trans.position = Vector3.Lerp(Trans.position, _TargetPos, SmoothFactor);
            Trans.rotation = Quaternion.Lerp(Trans.rotation, _TargetRot, SmoothFactor);
        }
    }
}
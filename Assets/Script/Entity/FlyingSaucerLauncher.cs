using System;
using Thunder.Sys;
using Thunder.Tool;
using UnityEngine;
using UnityEngine.Events;

namespace Thunder.Entity
{
    public class FlyingSaucerLauncher : BaseEntity
    {
        public float TurnSpeed;
        public float LaunchInterval;
        public float LaunchSpeed;
        public bool Enable;
        public Transform PlayerTrans;

        // todo 新发射方式等待测试
        public Vector3 LaunchBaseDir;
        public float LaunchRandAngle;
        public Vector2 LaunchForce;
        public PerlinNoise.PerlinParam LaunchDirNoise;
        public PerlinNoise.PerlinParam LaunchForceNoise;
        public LaunchModeE LaunchMode;
        public UnityEvent OnSaucerHit;

        private readonly PerlinNoise _LaunchNoise = new PerlinNoise();
        private Transform _Launcher;
        private float _LaunchTimeCount;

        public enum LaunchModeE
        {
            Circle,
            PlayerBased
        }

        protected override void Awake()
        {
            base.Awake();
            _Launcher = _Trans.Find("Launcher");
            _LaunchNoise.RegisterParam(LaunchDirNoise);
            _LaunchNoise.RegisterParam(LaunchForceNoise);
        }

        private Vector3 GetNextForceDir(Vector3 playerDir)
        {
            float limit = LaunchRandAngle;
            float perlin = _LaunchNoise.GetNext(0);
            limit = perlin * limit - limit / 2;
            Vector3 targetDir = Quaternion.AngleAxis(limit, Vector3.forward) * LaunchBaseDir;
            perlin = _LaunchNoise.GetNext(1);
            targetDir = targetDir.normalized * Mathf.Lerp(LaunchForce.x, LaunchForce.y, perlin);
            playerDir = playerDir.ProjectToxz();
            targetDir = Tools.BuildTransferMatrix(playerDir) * targetDir;

            return targetDir;
        }

        private void FixedUpdate()
        {
            if (!Enable)
                return;

            switch (LaunchMode)
            {
                case LaunchModeE.PlayerBased:
                    PlayerBasedLaunch();
                    break;
                case LaunchModeE.Circle:
                    CircleLaunch();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlayerBasedLaunch()
        {
            if (PlayerTrans == null) return;

            Vector3 force = GetNextForceDir(_Trans.position - PlayerTrans.position);
            _Launcher.position = _Trans.position + force.normalized;
            _Launcher.rotation = Quaternion.LookRotation(force);

            if (!(Time.time - _LaunchTimeCount > LaunchInterval)) return;
            _LaunchTimeCount = Time.time;

            Stable.ObjectPool.Alloc<FlyingSaucer>(null, null, "flyingSaucer", x =>
            {
                x.transform.position = _Launcher.position;
                x.Launch(force);
            });
        }

        private void CircleLaunch()
        {
            float angle = (Time.time * TurnSpeed) % 360;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 1, Mathf.Sin(angle));
            _Launcher.localPosition = dir;
            _Launcher.rotation = Quaternion.LookRotation(dir);

            if (!(Time.time - _LaunchTimeCount > LaunchInterval)) return;
            _LaunchTimeCount = Time.time;
            Vector3 force = _Trans.localToWorldMatrix * dir.normalized * LaunchSpeed;

            Stable.ObjectPool.Alloc<FlyingSaucer>(null, null, "flyingSaucer", x =>
            {
                x.transform.position = _Launcher.position;
                x.Launch(force);
            });
        }

        private void FlyingSaucerHit()
        {

        }
    }
}

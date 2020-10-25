using System;
using Thunder.Entity;
using Thunder.Utility;
using Tool;
using UnityEngine;
using UnityEngine.Events;

namespace Thunder.Game.FlyingSaucer
{
    public class FlyingSaucerLauncher : BaseEntity
    {
        public enum LaunchModeE
        {
            Circle,
            PlayerBased
        }

        private SimpleCounter _LaunchCounter;

        private PerlinNoise _LaunchDirNoise;
        private Transform _Launcher;
        private PerlinNoise _LaunchForceNoise;
        public bool Enable;

        public Vector3 LaunchBaseDir;
        public float LaunchDirRandomSmooth;
        public Vector2 LaunchForce;
        public float LaunchForceRandomSmooth;
        public float LaunchInterval;
        public LaunchModeE LaunchMode;
        public float LaunchRandAngle;
        public float LaunchSpeed;
        public UnityEvent OnSaucerHit;
        public Transform PlayerTrans;
        public float TurnSpeed;

        protected override void Awake()
        {
            base.Awake();
            _Launcher = _Trans.Find("Launcher");
            _LaunchDirNoise = new PerlinNoise(LaunchDirRandomSmooth);
            _LaunchForceNoise = new PerlinNoise(LaunchForceRandomSmooth);

            PublicEvents.GameEnd.AddListener((x, y) =>
            {
                if (x == GameType.FlyingSaucer)
                    Enable = false;
            });
            _LaunchCounter = new SimpleCounter(LaunchInterval);
        }

        private Vector3 GetNextForceDir(Vector3 playerFaceDir)
        {
            var limit = LaunchRandAngle;
            var perlin = _LaunchDirNoise.GetNext();
            limit = perlin * limit - limit / 2;
            var targetDir = Quaternion.AngleAxis(limit, Vector3.forward) * LaunchBaseDir;
            perlin = _LaunchForceNoise.GetNext();
            targetDir = targetDir.normalized * Mathf.Lerp(LaunchForce.x, LaunchForce.y, perlin);
            targetDir = Tools.BuildTransferMatrix(playerFaceDir.ProjectToxz()) * targetDir;

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

            if (!_LaunchCounter.Completed) return;
            _LaunchCounter.Recount();

            var force = GetNextForceDir(PlayerTrans.rotation * Vector3.forward);
            _Launcher.position = _Trans.position + force.normalized;
            _Launcher.rotation = Quaternion.LookRotation(force);

            var saucer = ObjectPool.Ins.Alloc<FlyingSaucer>("flyingSaucer");
            saucer.transform.position = _Launcher.position;
            saucer.Launch(force);
        }

        private void CircleLaunch()
        {
            var angle = Time.time * TurnSpeed % 360;
            var dir = new Vector3(Mathf.Cos(angle), 1, Mathf.Sin(angle));
            _Launcher.localPosition = dir;
            _Launcher.rotation = Quaternion.LookRotation(dir);

            if (!_LaunchCounter.Completed) return;
            _LaunchCounter.Recount();
            Vector3 force = _Trans.localToWorldMatrix * dir.normalized * LaunchSpeed;

            var saucer = ObjectPool.Ins.Alloc<FlyingSaucer>("flyingSaucer");
            saucer.transform.position = _Launcher.position;
            saucer.Launch(force);
        }
    }
}
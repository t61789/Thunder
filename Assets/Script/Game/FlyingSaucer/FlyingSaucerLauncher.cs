using System;
using Thunder.Entity;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Thunder.Game.FlyingSaucer
{
    public class FlyingSaucerLauncher : BaseEntity
    {
        public int DelayTime;
        public float TurnSpeed;
        public float LaunchInterval;
        public float LaunchSpeed;
        public bool Enable;
        public Transform PlayerTrans;

        // todo 新发射方式等待测试
        public Vector3 LaunchBaseDir;
        public float LaunchRandAngle;
        public Vector2 LaunchForce;
        public float LaunchDirRandomSmooth;
        public float LaunchForceRandomSmooth;
        public LaunchModeE LaunchMode;
        public UnityEvent OnSaucerHit;

        private PerlinNoise _LaunchDirNoise;
        private PerlinNoise _LaunchForceNoise;
        private Transform _Launcher;
        private Counter _StartLaunchCounter;
        private int _Countdown;
        private Counter _LaunchCounter;

        public enum LaunchModeE
        {
            Circle,
            PlayerBased
        }

        protected override void Awake()
        {
            base.Awake();
            _Launcher = _Trans.Find("Launcher");
            _LaunchDirNoise = new PerlinNoise(LaunchDirRandomSmooth);
            _LaunchForceNoise = new PerlinNoise(LaunchForceRandomSmooth);

            _StartLaunchCounter = new Counter(0).ToAutoCounter(this,false);
            PublicEvents.FlyingSaucerGameStartDelay.AddListener(() => StartLaunchDelay(DelayTime));
            PublicEvents.FlyingSaucerGameEnd.AddListener(x => Enable = false);
            _LaunchCounter = new Counter(LaunchInterval);
        }

        private Vector3 GetNextForceDir(Vector3 playerFaceDir)
        {
            float limit = LaunchRandAngle;
            float perlin = _LaunchDirNoise.GetNext();
            limit = perlin * limit - limit / 2;
            Vector3 targetDir = Quaternion.AngleAxis(limit, Vector3.forward) * LaunchBaseDir;
            perlin = _LaunchForceNoise.GetNext();
            targetDir = targetDir.normalized * Mathf.Lerp(LaunchForce.x, LaunchForce.y, perlin);
            targetDir = Tools.BuildTransferMatrix(playerFaceDir.ProjectToxz()) * targetDir;

            return targetDir;
        }

        private void FixedUpdate()
        {
            if (_Countdown != 0 && _StartLaunchCounter.TimeCount > DelayTime - _Countdown)
            {
                LogPanel.Instance.LogSystem($"Game will start in {_Countdown} seconds");
                _Countdown--;
            }

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

            Vector3 force = GetNextForceDir(PlayerTrans.rotation * Vector3.forward);
            _Launcher.position = _Trans.position + force.normalized;
            _Launcher.rotation = Quaternion.LookRotation(force);

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

            if (!_LaunchCounter.Completed) return;
            _LaunchCounter.Recount();
            Vector3 force = _Trans.localToWorldMatrix * dir.normalized * LaunchSpeed;

            Stable.ObjectPool.Alloc<FlyingSaucer>(null, null, "flyingSaucer", x =>
            {
                x.transform.position = _Launcher.position;
                x.Launch(force);
            });
        }

        public void StartLaunchDelay(int delay = -1)
        {
            _StartLaunchCounter.TimeLimit = delay;
            _StartLaunchCounter.OnComplete(() =>
            {
                Enable = true;
                PublicEvents.FlyingSaucerGameStart?.Invoke();
                _StartLaunchCounter.PauseAutoCount();
                LogPanel.Instance.LogSystem("Game start!!!");
            }).ResumeAutoCount(true);
            _Countdown = delay;
        }
    }
}

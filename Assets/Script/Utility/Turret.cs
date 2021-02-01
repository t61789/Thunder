using System.Collections.Generic;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class Turret:BaseBuilding
    {
        public Transform Head;
        public float FireInterval = 0.1f;
        public float FireColdDown = 0.7f;
        public int BurstTime = 7;
        public float Damage = 10;
        public float RotSpeed = 60f;
        public int FireAudioIndex = 0;
        public Transform[] FirePos;
        public float MuzzleFireLifeTime = 0.1f;
        public string MuzzleFirePrefabPath = "prefabs/normal/muzzleFire";
        public string GunFireSmokePrefabPath = "prefabs/normal/gunFireSmoke";
        
        private BaseCharacter _Target;
        private RangedWeaponLauncher _RangedWeaponLauncher;
        private SimpleCounter _FireColdDownCounter;
        private SimpleCounter _FireIntervalCounter;
        private int _BurstCount = 0;
        private SphereCollider _CheckRange;
        private AudioController _AudioController;
        private int _FirePosCount;
        private CommonMuzzleFire[] _MuzzleFires;
        private ParticleSystem[] _GunFireSmokes; 

        protected override void Awake()
        {
            base.Awake();

            _AudioController = GetComponent<AudioController>();
            _RangedWeaponLauncher = this.GetComponentAndLog<RangedWeaponLauncher>($"{name} 未指定发射器");
            _CheckRange = GetComponent<SphereCollider>();
            _RangedWeaponLauncher.OnHit += BulletHit;

            _MuzzleFires = new CommonMuzzleFire[FirePos.Length];
            _GunFireSmokes = new ParticleSystem[FirePos.Length];
            for (int i = 0; i < FirePos.Length; i++)
            {
                var go = GameObjectPool.Get<CommonMuzzleFire>(MuzzleFirePrefabPath);
                go.Init(FirePos[i], Vector3.zero, Quaternion.identity);
                _MuzzleFires[i] = go;

                var particle = GameObjectPool.GetPrefab(GunFireSmokePrefabPath).GetInstantiate().transform;
                particle.SetParent(FirePos[i]);
                particle.localPosition = Vector3.zero;
                particle.localRotation = Quaternion.identity;
                _GunFireSmokes[i] = particle.GetComponent<ParticleSystem>();
            }

            _FireColdDownCounter = new SimpleCounter(FireColdDown).Complete();
            _FireIntervalCounter = new SimpleCounter(FireInterval).Complete();
        }

        private void FixedUpdate()
        {
            KeepFire();

            RotateToTarget();

            CheckTargetRange();
        }

        private void OnTriggerEnter(Collider col)
        {
            if (!col.TryGetComponent<BaseCharacter>(out var character)) return;
            if (!CampSys.IsHate(Camp, character.Camp)) return;

            SetTarget(character);
            _CheckRange.enabled = false;
        }

        private void OnDestroy()
        {
            _RangedWeaponLauncher.OnHit -= BulletHit;
            SetTarget(null);
        }

        private void KeepFire()
        {
            if (_Target == null) return;

            if (_BurstCount <= 0 && _FireColdDownCounter.Completed)
            {
                _FireColdDownCounter.Recount();
                _BurstCount = BurstTime;
            }

            if (_BurstCount > 0 && _FireIntervalCounter.Completed)
            {
                _FireIntervalCounter.Recount();
                _BurstCount--;

                if(_BurstCount<=0)
                    _FireColdDownCounter.Recount();

                _AudioController.PlayAudio(FireAudioIndex);
                _RangedWeaponLauncher?.FireOnce(FirePos[_FirePosCount].position, FirePos[_FirePosCount].forward);
                _MuzzleFires[_FirePosCount].Show();
                _GunFireSmokes[_FirePosCount].Play();
                _FirePosCount = (_FirePosCount + 1) % FirePos.Length;
            }
        }

        private void RotateToTarget()
        {
            Vector3 dir;
            if (_Target == null)
            {
                dir = Vector3.forward;
            }
            else
            {
                dir = _Target.Trans.position - Trans.position;
            }

            Head.rotation = Quaternion.RotateTowards(
                Head.rotation,
                Quaternion.LookRotation(dir),
                RotSpeed*Time.fixedDeltaTime);
        }

        private void CheckTargetRange()
        {
            if (_Target == null) return;

            var distance = (_Target.Trans.position - Trans.position).sqrMagnitude;
            var radius = _CheckRange.radius;
            if (distance > radius* radius)
            {
                _Target = null;
                _CheckRange.enabled = true;
            }
        }

        private void SetTarget(BaseCharacter target)
        {
            if (_Target != null)
                _Target.Health.ReachMin -= TargetDead;

            if (target == null)
                return;

            _Target = target;
            _Target.Health.ReachMin += TargetDead;
        }

        private void TargetDead()
        {
            _Target = null;
            _CheckRange.enabled = true;
        }

        private void BulletHit(IEnumerable<HitInfo> hitInfos)
        {
            foreach (var hitInfo in hitInfos)
            {
                if(!hitInfo.Collider.TryGetComponent<BaseCharacter>(out var character))continue;
                character.GetHit(hitInfo.HitPos,hitInfo.HitDir, Damage);
            }
        }
    }
}

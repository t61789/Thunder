using System;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class MachineGunFireControl:FireControl
    {
        public bool LogWarning;
        public bool Safety;
        public float FireInterval;
        public CtrKeyRequest FireCtr;
        public Transform AimBase;
        public Func<bool> FireCheck;
        [SerializeField] private FireBurst _Burst;

        private readonly SimpleCounter _ShootCounter;

        public float OverHeat => _RangedWeaponLauncher.OverHeat;

        public MachineGunFireControl(RangedWeaponLauncher launcher):base(launcher)
        {
            _ShootCounter = new SimpleCounter(FireInterval);
            _Burst.Init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>射击信息</returns>
        public override object TryFire(ControlInfo input)
        {
            var result = new ShootInfo
            {
                SafetyPass = !Safety,
                IntervalPass = _ShootCounter.Completed,
                FireRequirePass = _Burst.RequireFire(input)
            };

            if (FireCheck == null)
            {
                if(LogWarning)
                    Debug.LogWarning($"未设定开火条件函数");
            }
            else
            {
                result.FireCheckPass = FireCheck();
            }

            result.Shooted = 
                result.SafetyPass &&
                result.IntervalPass && 
                result.FireCheckPass && 
                result.FireRequirePass;

            if (result.Shooted)
            {
                var forward = AimBase.forward;
                _RangedWeaponLauncher.FireOnce(AimBase.position, forward);
                _ShootCounter.Recount();
            }

            return result;
        }

        public void Resets()
        {
            Safety = true;
            _ShootCounter.Complete();
        }

        public void OpenSafety()
        {
            Safety = false;
        }

        public void CloseSafety()
        {
            Safety = true;
        }

        public void LoopBurstMode()
        {
            _Burst.LoopMode();
        }

        public void SetLauncher(RangedWeaponLauncher launcher)
        {
            _RangedWeaponLauncher = launcher;
        }
    }

    //public struct ShootInfo
    //{
    //    public bool IntervalPass;
    //    public bool FireCheckPass;
    //    public bool FireRequirePass;
    //    public bool SafetyPass;
    //    public bool Shooted;
    //    public bool HitSomething;
    //    public Vector3 ShootDir;
    //    public RaycastHit HitInfo;
    //}

    [Serializable]
    public class FireBurst
    {
        public int BurstMode;
        public int[] FireModeLoop = { 0, 1, 3 };

        private int _BurstCount;

        public void Init()
        {
            Reset();
        }

        public void Reset()
        {
            _BurstCount = 0;
            BurstMode = FireModeLoop[0];
        }

        public void LoopMode()
        {
            var index = FireModeLoop.FindIndex(x => BurstMode == x);
            index++;
            index %= FireModeLoop.Length;
            BurstMode = FireModeLoop[index];
            PublicEvents.GunFireModeChange?.Invoke(BurstMode);
        }

        public bool RequireFire(ControlInfo controlInfo)
        {
            if (BurstMode == 0)
            {
                return controlInfo.Stay;
            }

            if (_BurstCount == 0)
            {
                if (controlInfo.Down)
                    _BurstCount = BurstMode;
                else
                    return false;
            }

            _BurstCount--;
            return true;
        }
    }
}

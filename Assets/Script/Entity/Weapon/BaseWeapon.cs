using System;
using System.CodeDom;
using System.Collections.Generic;
using Framework;

using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder
{
    public abstract class BaseWeapon : BaseEntity, IItem
    {
        public bool FireOnlyAmmoEnough;
        public int FireCostAmmo = 1;
        public AmmoGroup AmmoGroup;
        public bool Safety;
        public Func<bool> FireCheck = () => true;
        public event Action<Vector2> OnFixedRecoil;
        [SerializeField] private float _FireInterval;
        [SerializeField] private FireBurst _Burst;

        public float FireInterval
        {
            get => _FireInterval;
            set
            {
                _FireInterval = value;
                _ShootCounter.Recount(_FireInterval);
            }
        }
        public float OverHeat => _RangedWeaponLauncher.OverHeat;

        protected Player player;

        private RangedWeaponLauncher _RangedWeaponLauncher;
        private SimpleCounter _ShootCounter;

        public ItemId ItemId { get; set; }

        public abstract float OverHeatFactor { get; }

        public virtual void Init(Transform weaponContainer,string addData)
        {
            Trans.SetParent(weaponContainer);
            Trans.localPosition = Vector3.zero;
            Trans.localRotation = Quaternion.identity;
            player = Trans.parent.parent.parent.GetComponent<Player>();
            if (player == null)
                throw new Exception($"武器 {name} 安装位置不正确");
            _ShootCounter = new SimpleCounter(FireInterval);
            _Burst.Init();
            AmmoGroup.Init(player.Package);
            SetLauncher(GetComponent<RangedWeaponLauncher>());

            DecompressItem(addData);
        }

        protected virtual void OnDestroy()
        {
            AmmoGroup.Destroy();
        }

        public void SetSafety(int i)
        {
            switch (i)
            {
                case 0:
                    CloseSafety();
                    break;
                case 1:
                    OpenSafety();
                    break;
            }
        }

        /// <summary>
        /// 打开保险，不能击发
        /// </summary>
        public void OpenSafety()
        {
            Safety = false;
        }

        /// <summary>
        /// 关闭保险，可以击发
        /// </summary>
        public void CloseSafety()
        {
            Safety = true;
        }

        /// <summary>
        /// 循环切换连发模式
        /// </summary>
        public void LoopBurstMode()
        {
            _Burst.LoopMode();
        }

        /// <summary>
        /// 设置发射器
        /// </summary>
        /// <param name="launcher"></param>
        public void SetLauncher(RangedWeaponLauncher launcher)
        {
            if(_RangedWeaponLauncher==null)
            {
                if (launcher == null)
                    Debug.Log($"{name} 未指定发射器");
                return;
            }
            Destroy(_RangedWeaponLauncher);

            _RangedWeaponLauncher = launcher;
            _RangedWeaponLauncher.OnHit = OnHit;
        }

        /// <summary>
        /// 进行一次击发
        /// </summary>
        public abstract void Fire();

        /// <summary>
        /// 装弹
        /// </summary>
        public abstract void Reload();

        /// <summary>
        /// 武器取出时调用
        /// </summary>
        public abstract void TakeOut();

        /// <summary>
        /// 武器收起时调用
        /// </summary>
        public abstract void PutBack();

        /// <summary>
        /// 将武器压缩为物品
        /// </summary>
        /// <returns>物品的附加参数</returns>
        public abstract string CompressItem();

        /// <summary>
        /// 读取物品附加数据
        /// </summary>
        /// <param name="add"></param>
        public abstract void DecompressItem(string add);

        /// <summary>
        /// 获取当前的浮动后坐力
        /// </summary>
        /// <returns></returns>
        public virtual Vector2 GetFloatRecoil()
        {
            return Vector2.zero;
        }

        /// <summary>
        /// 子弹击中时触发
        /// </summary>
        /// <returns></returns>
        protected abstract void OnHit(IEnumerable<HitInfo> hitInfos);

        /// <summary>
        /// 操作一次扳机
        /// </summary>
        /// <returns>射击信息</returns>
        protected FireInfo ShootTrigger(Transform aimBase, bool triggerDown, bool triggerStay)
        {
            var result = FireInfo.Take();
            result.SafetyPass = !Safety;
            result.FireCheckPass = FireCheck();
            result.IntervalPass = _ShootCounter.Completed;
            result.BurstRequirePass = _Burst.Check(triggerDown, triggerStay);
            result.AmmoPass = AmmoGroup.CostAmmoCheck(FireCostAmmo, FireOnlyAmmoEnough);
            result.HasShot = result.SafetyPass &&
                             result.FireCheckPass &&
                             result.IntervalPass &&
                             result.BurstRequirePass &&
                             result.AmmoPass;

            if (!result.HasShot)
            {
                _Burst.RollBack();
                return result;
            }

            _Burst.Confirm();
            AmmoGroup.CostAmmo(FireCostAmmo, FireOnlyAmmoEnough);

            var forward = aimBase.forward;
            _RangedWeaponLauncher?.FireOnce(aimBase.position, forward);
            _ShootCounter.Recount();

            return result;
        }

        /// <summary>
        /// 触发一次固定后坐力
        /// </summary>
        /// <param name="obj"></param>
        protected void InvokeOnFixedRecoil(Vector2 obj)
        {
            OnFixedRecoil?.Invoke(obj);
        }
    }

    public class RecoilInfo : ObjectQueueItem<RecoilInfo>
    {
        public Vector2 FloatRecoil;
        public Vector2 FixedRecoil;

        protected override void Reset()
        {
            FloatRecoil = Vector2.zero;
            FixedRecoil = Vector2.zero;
        }
    }

    public class FireInfo : ObjectQueueItem<FireInfo>
    {
        public bool IntervalPass;
        public bool FireCheckPass;
        public bool BurstRequirePass;
        public bool AmmoPass;
        public bool SafetyPass;
        public bool HasShot;

        protected override void Reset()
        {
            IntervalPass = false;
            FireCheckPass = false;
            BurstRequirePass = false;
            SafetyPass = false;
            HasShot = false;
            AmmoPass = false;
        }
    }

    [Serializable]
    public class FireBurst
    {
        public bool Triggerable = true;
        public int BurstMode;
        public int[] FireModeLoop = { 0, 1, 3 };

        private int _BurstCount;
        private int _SaveBurstCount = -2;

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

        public bool Check(bool triggerDown, bool triggerStay)
        {
            if (!Triggerable)
                return false;

            if (BurstMode == 0)
            {
                return triggerStay;
            }

            if (_BurstCount == 0)
            {
                if (triggerDown)
                    _SaveBurstCount = BurstMode;
                else
                    return false;
            }

            _SaveBurstCount = _BurstCount - 1;
            return true;
        }

        public void Confirm()
        {
            if (_SaveBurstCount == -2) return;
            _BurstCount = _SaveBurstCount;
            _SaveBurstCount = -2;
        }

        public void RollBack()
        {
            _SaveBurstCount = -2;
        }
    }

    [Serializable]
    public class AmmoGroup
    {
        /// <summary>
        /// 子弹Id
        /// </summary>
        public int AmmoId
        {
            private set => _AmmoId = value;
            get => _AmmoId;
        }
        [SerializeField] private int _AmmoId;
        /// <summary>
        /// 当前弹匣中的子弹数
        /// </summary>
        public int Magazine;
        /// <summary>
        /// 弹容量
        /// </summary>
        public int MagazineMax
        {
            private set => _MagazineMax = value;
            get => _MagazineMax;
        }
        [SerializeField] private int _MagazineMax;
        /// <summary>
        /// 后备弹药数量，从背包中读取
        /// </summary>
        public int BackupAmmo => Package.GetItemNum(AmmoId);
        /// <summary>
        /// 关联的背包
        /// </summary>
        public CommonPackage Package { get; set; }

        /// <summary>
        /// 子弹数量变化时触发
        /// </summary>
        public event Action<AmmoGroup> OnAmmoChanged
        {
            add
            {
                _OnAmmoChanged += value;
                InvokeOnAmmoCellChanged(default);
            }

            remove => _OnAmmoChanged -= value;
        }
        private event Action<AmmoGroup> _OnAmmoChanged;

        /// <summary>
        /// 弹匣中的子弹的种类
        /// </summary>
        private int _AmmoIdInMagazine;

        public void Init(CommonPackage package)
        {
            _AmmoIdInMagazine = _AmmoId;
            Package = package;
            Package.OnItemChanged += InvokeOnAmmoCellChanged;
        }

        public void Destroy()
        {
            Package.OnItemChanged -= InvokeOnAmmoCellChanged;
        }

        /// <summary>
        ///     判断是否可以装弹
        /// </summary>
        /// <returns></returns>
        public bool ReloadConfirm()
        {
            if (_AmmoIdInMagazine != AmmoId) return true;
            return Magazine < MagazineMax && BackupAmmo != 0;
        }

        /// <summary>
        /// 从背包中取出子弹装入弹匣
        /// </summary>
        /// <returns>是否成功装弹</returns>
        public bool Reload()
        {
            if (_AmmoIdInMagazine != AmmoId)
            {
                Package.PutItem((_AmmoIdInMagazine, Magazine));
                Magazine = 0;
                _AmmoIdInMagazine = AmmoId;
            }
            if (!ReloadConfirm()) return false;

            var ammoDiff = Mathf.Min(MagazineMax - Magazine, BackupAmmo);
            Magazine += ammoDiff;
            Package.CostItem((AmmoId, ammoDiff));
            InvokeOnAmmoCellChanged(default);
            return true;
        }

        /// <summary>
        ///     判断弹匣是否为空
        /// </summary>
        /// <returns></returns>
        public bool MagazineEmpty()
        {
            return Magazine <= 0;
        }

        /// <summary>
        /// 消耗弹匣中的子弹
        /// </summary>
        /// <param name="costNum">一次性小号的子弹数</param>
        /// <param name="costOnlyAmmoEnough">是否仅在弹匣中弹药足够时才消耗</param>
        /// <returns>缺少多少弹药</returns>
        public int CostAmmo(int costNum, bool costOnlyAmmoEnough)
        {
            var temp = Magazine - costNum;
            if (temp < 0)
            {
                if (!costOnlyAmmoEnough)
                    Magazine = 0;
                InvokeOnAmmoCellChanged(default);
                return -temp;
            }

            Magazine = temp;
            InvokeOnAmmoCellChanged(default);
            return 0;
        }

        /// <summary>
        /// 检查弹匣内的子弹是否足够一次射击
        /// </summary>
        /// <param name="costNum"></param>
        /// <param name="passOnlyAmmoEnough">仅在子弹完全足够时通过</param>
        /// <returns></returns>
        public bool CostAmmoCheck(int costNum, bool passOnlyAmmoEnough)
        {
            if (passOnlyAmmoEnough)
                return Magazine >= costNum;
            return Magazine > 0;
        }

        /// <summary>
        /// 设置弹容量
        /// </summary>
        /// <param name="magazineMax">弹容量</param>
        /// <param name="discardExtraPart">是否抛弃多出的部分</param>
        public void SetMagazineMax(int magazineMax, bool discardExtraPart)
        {
            if (magazineMax < 0)
                throw new Exception($"弹容量设置错误 {magazineMax}");
            InvokeOnAmmoCellChanged(default);
            MagazineMax = magazineMax;
            if (discardExtraPart && Magazine > MagazineMax)
                Magazine = MagazineMax;
        }

        /// <summary>
        /// 设置要使用的弹药
        /// </summary>
        /// <param name="ammoId"></param>
        public void SetAmmoId(int ammoId)
        {
            AmmoId = ammoId;
            InvokeOnAmmoCellChanged(default);
        }

        public void InvokeOnAmmoCellChanged(PackageItemChangedInfo a)
        {
            _OnAmmoChanged?.Invoke(this);
        }
    }
}
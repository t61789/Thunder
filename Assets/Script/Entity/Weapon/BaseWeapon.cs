using System;
using Thunder.Sys;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Entity.Weapon
{
    public abstract class BaseWeapon : BaseEntity, IItem
    {
        public static BaseWeapon Ins;

        protected Player _Player;
        public AmmoGroup AmmoGroup;

        public ItemId ItemId { get; set; }

        protected override void Awake()
        {
            base.Awake();

            Ins = this;
            _Player = _Trans.parent.parent.parent.GetComponent<Player>();
            Assert.IsNotNull(_Player,
                $"武器 {name} 安装位置不正确");
            _Player.OnSquat.AddListener(PlayerSquat);
            _Player.OnHanging.AddListener(PlayerHanging);
        }

        protected virtual void OnEnable()
        {
            TakeOut();
        }

        protected abstract void PlayerSquat(bool squatting, bool hanging);

        protected abstract void PlayerHanging(bool squatting, bool hanging);

        public abstract void Fire();

        public abstract void Reload();

        public abstract void TakeOut();

        public abstract void PutBack();

        public abstract object Drop();

        public abstract void ReadAdditionalData(object add);
    }

    [Serializable]
    public class AmmoGroup
    {
        public int BackupAmmo => _Package.GetItemNum(AmmoId);
        public int AmmoId { get; }

        public int Magzine;
        public int MagzineMax;
        private readonly Package _Package;

        public event Action<AmmoGroup> OnAmmoChanged;

        public AmmoGroup(int magzineMax, int ammoId,Package package)
        {
            MagzineMax = magzineMax;
            Magzine = magzineMax;
            _Package = package;
            AmmoId = ammoId;
        }

        /// <summary>
        ///     判断是否可以装弹
        /// </summary>
        /// <returns></returns>
        public bool ReloadConfirm()
        {
            return Magzine != MagzineMax && BackupAmmo != 0;
        }

        /// <summary>
        ///     从背包中取出子弹装入弹匣
        /// </summary>
        public void Reload()
        {
            var ammoDiff = Mathf.Min(MagzineMax - Magzine, BackupAmmo);
            Magzine += ammoDiff;
            _Package.CostItem(AmmoId, ammoDiff,out _);
        }

        /// <summary>
        ///     判断弹匣是否为空
        /// </summary>
        /// <returns></returns>
        public bool MagzineEmpty()
        {
            return Magzine == 0;
        }

        public void InvokeOnAmmoChanged()
        {
            OnAmmoChanged?.Invoke(this);
        }
    }
}
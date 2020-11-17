using System;
using Framework;

using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder
{
    public abstract class BaseWeapon : BaseEntity, IItem
    {
        public static BaseWeapon Ins;
        public AmmoGroup AmmoGroup;

        protected Player _Player;

        public ItemId ItemId { get; set; }

        public abstract float OverHeatFactor { get; }

        protected override void Awake()
        {
            base.Awake();

            Ins = this;
            _Player = Trans.parent.parent.parent.GetComponent<Player>();
            Assert.IsNotNull(_Player,
                $"武器 {name} 安装位置不正确");
        }

        public abstract void Fire();

        public abstract void Reload();

        public abstract void TakeOut();

        public abstract void PutBack();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>物品的附加参数</returns>
        public abstract ItemAddData Drop();

        public abstract void ReadAdditionalData(ItemAddData add);
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
            _Package.CostItem((AmmoId, ammoDiff),out _);
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
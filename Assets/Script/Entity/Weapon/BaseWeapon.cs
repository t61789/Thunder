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

        protected Player player;

        public ItemId ItemId { get; set; }

        public abstract float OverHeatFactor { get; }

        protected override void Awake()
        {
            base.Awake();

            Ins = this;
            player = Trans.parent.parent.parent.GetComponent<Player>();
            Assert.IsNotNull(player,
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

        public int Magazine;
        public int MagazineMax;
        private readonly Package _Package;

        public event Action<AmmoGroup> OnAmmoChanged;

        public AmmoGroup(int magazineMax, int ammoId,Package package)
        {
            this.MagazineMax = magazineMax;
            Magazine = magazineMax;
            _Package = package;
            AmmoId = ammoId;
        }

        /// <summary>
        ///     判断是否可以装弹
        /// </summary>
        /// <returns></returns>
        public bool ReloadConfirm()
        {
            return Magazine != MagazineMax && BackupAmmo != 0;
        }

        /// <summary>
        ///     从背包中取出子弹装入弹匣
        /// </summary>
        public void Reload()
        {
            var ammoDiff = Mathf.Min(MagazineMax - Magazine, BackupAmmo);
            Magazine += ammoDiff;
            _Package.CostItem((AmmoId, ammoDiff),out _);
        }

        /// <summary>
        ///     判断弹匣是否为空
        /// </summary>
        /// <returns></returns>
        public bool MagazineEmpty()
        {
            return Magazine == 0;
        }

        public void InvokeOnAmmoChanged()
        {
            OnAmmoChanged?.Invoke(this);
        }
    }
}
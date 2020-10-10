using System;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Entity.Weapon
{
    public abstract class BaseWeapon : BaseEntity, IItem
    {
        public static BaseWeapon Ins;

        [SerializeField] private int _ItemId;

        protected Player _Player;
        public AmmoGroup AmmoGroup;
        public int ItemId => _ItemId;

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

        public abstract void FillAmmo();

        public abstract void TakeOut();

        public abstract void PutBack();

        public abstract void Drop();
    }

    [Serializable]
    public class AmmoGroup
    {
        public int Backup;
        public int BackupMax;
        public int Magzine;
        public int MagzineMax;

        public AmmoGroup(int magzineMax, int magzine, int backupMax, int backup)
        {
            MagzineMax = magzineMax;
            Magzine = magzine;
            BackupMax = backupMax;
            Backup = backup;
        }

        public event Action<AmmoGroup> OnAmmoChanged;

        /// <summary>
        ///     判断是否可以装弹
        /// </summary>
        /// <returns></returns>
        public bool ReloadConfirm()
        {
            return Magzine != MagzineMax && Backup != 0;
        }

        /// <summary>
        ///     从后被弹药中取出子弹装入弹匣
        /// </summary>
        public void Reload()
        {
            var ammoDiff = Mathf.Min(MagzineMax - Magzine, Backup);
            Magzine += ammoDiff;
            Backup -= ammoDiff;
        }

        /// <summary>
        ///     从弹药箱中填充所有子弹
        /// </summary>
        /// <param name="backupOnly">是否只填满备用弹药</param>
        public void FillUp(bool backupOnly)
        {
            Backup = BackupMax;
            if (!backupOnly)
                Magzine = MagzineMax;
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
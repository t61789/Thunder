using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BehaviorDesigner.Runtime;
using Thunder.Entity.Weapon;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Thnder.Utility
{
    public class  WeaponBelt
    {
        // todo 亟待测试，bug可能很多
        private readonly Transform _WeaponContainer;
        private int _CurWeapon;
        private readonly BaseWeapon[] _Belt;
        private int _PreWeapon = -1;

        private const int STATIC_INDEX = 0;

        public WeaponBelt(int capacity,Transform weaponContainer)
        {
            _Belt = new BaseWeapon[capacity];
            _WeaponContainer = weaponContainer;

            AddWeapon(STATIC_INDEX,GlobalSettings.UnarmedAssetPath);
            _CurWeapon = STATIC_INDEX;
        }

        public void SwitchWeapon(int index)
        {
            if (_Belt[index] == null || index==_CurWeapon) return;
            PutBackWeapon(_CurWeapon);
            TakeOutWeapon(index);
            _PreWeapon = _CurWeapon;
            _CurWeapon = index;
        }

        public void SwitchWeaponToPre()
        {
            if (_PreWeapon == -1) return;
            SwitchWeapon(_PreWeapon);
        }

        public void AddWeapon(int index, string prefabPath)
        {
            if (_Belt[index]!=null)
                DestroyWeapon(index);
            var newWeapon = BundleSys.Ins.GetAsset<GameObject>(prefabPath).
                GetInstantiate().GetComponent<BaseWeapon>();
            newWeapon.Trans.SetParent(_WeaponContainer);
            _Belt[index] = newWeapon;
            if(index==_CurWeapon)
                TakeOutWeapon(index);
            else
                PutBackWeapon(index);
        }

        public void DropWeapon()
        {
            if (_CurWeapon == STATIC_INDEX) return;
            DestroyWeapon(_CurWeapon);
            if(_PreWeapon!=-1)
                TakeOutWeapon(_PreWeapon);
            else
                while (_Belt[_CurWeapon] == null)
                {
                    _CurWeapon++;
                    _CurWeapon %= _Belt.Length;
                }

            _PreWeapon = -1;
            TakeOutWeapon(_CurWeapon);

            //todo 丢出可碰撞的模型
        }

        private void TakeOutWeapon(int index)
        {
            PutBackWeapon(_CurWeapon);
            _Belt[index].gameObject.SetActive(true);
            _Belt[index].TakeOut();
            _CurWeapon = index;
        }

        private void PutBackWeapon(int index)
        {
            _Belt[index].PutBack();
            _Belt[index].gameObject.SetActive(false);
        }

        private void DestroyWeapon(int index)
        {
            if(_CurWeapon==index)
                PutBackWeapon(index);
            Object.Destroy(_Belt[index].gameObject);
            _Belt[index] = null;
        }
    }

    public class WeaponBeltInput
    {
        private readonly WeaponBelt _WeaponBelt;
        private readonly string[] _KeyDic;
        private const int SHIELD_VALUE = 0;

        public WeaponBeltInput(WeaponBelt weaponBelt)
        {
            _WeaponBelt = weaponBelt;
            _KeyDic = new []
            {
                GlobalSettings.MainWeapon1KeyName,
                GlobalSettings.MainWeapon2KeyName,
                GlobalSettings.SecondaryKeyName,
                GlobalSettings.MeleeWeaponKeyName,
                GlobalSettings.ThrowingWeaponKeyName
            };
        }

        public void InputCheck()
        {
            if (ControlSys.Ins.RequireKey(GlobalSettings.PreWeaponKeyName, SHIELD_VALUE).Down)
            {
                _WeaponBelt.SwitchWeaponToPre();
            }
            else
                for (int i=0;i<_KeyDic.Length;i++)
                {
                    if (!ControlSys.Ins.RequireKey(_KeyDic[i], SHIELD_VALUE).Down) continue;
                    _WeaponBelt.SwitchWeapon(i+1);
                    break;
                }

            if (ControlSys.Ins.RequireKey(GlobalSettings.DropWeaponKeyName, SHIELD_VALUE).Down)
            {
                _WeaponBelt.DropWeapon();
            }
        }
    }
}

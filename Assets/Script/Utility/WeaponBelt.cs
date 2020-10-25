using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thunder.Entity.Weapon;
using Tool;


using UnityEngine;

namespace Thunder.Utility
{
    public class WeaponBelt
    {
        private const int SHIELD_VALUE = 0;
        private readonly WeaponBeltCell[] _Belt;
        private readonly Dictionary<string, int> _Keys;
        private readonly BaseWeapon _Unarmed;

        // todo 亟待测试，bug可能很多
        private readonly Transform _WeaponContainer;
        private readonly Dictionary<int, WeaponInfo> _WeaponInfoDic;
        private int _CurWeapon = -1;
        private int _PreWeapon = -1;

        public WeaponBelt(string[] cellTypes, Transform weaponContainer)
        {
            var builder = new StringBuilder();
            const string switc = "Switch";
            _Keys = new Dictionary<string, int>();
            _Belt = new WeaponBeltCell[cellTypes.Length];
            var preStr = "";
            var repeatCount = 1;
            for (var i = 0; i < cellTypes.Length; i++)
            {
                _Belt[i].Type = cellTypes[i];

                builder.Clear();
                builder.Append(switc);
                var str = cellTypes[i];

                if (str == preStr)
                {
                    repeatCount++;
                }
                else
                {
                    repeatCount = 1;
                    preStr = str;
                }

                if (str.Length > 0 && str[0] >= 'a' && str[0] <= 'z')
                {
                    builder.Append(str[0] - ('a' - 'A'));
                    if (str.Length > 1)
                        builder.Append(str.Substring(1));
                }
                else
                {
                    builder.Append(str);
                }

                builder.Append(repeatCount);
                _Keys.Add(builder.ToString(), i);
            }

            _Unarmed = CreateWeapon(GlobalSettings.UnarmedId);

            _WeaponContainer = weaponContainer;
            _WeaponInfoDic = QueryDic();
        }

        public BaseWeapon CurrentWeapon => _CurWeapon == -1 ? _Unarmed : _Belt[_CurWeapon].Weapon;

        /// <summary>
        ///     切换为目标单元格内的武器
        /// </summary>
        /// <param name="index"></param>
        public void SwitchWeapon(int index)
        {
            if (_Belt[index].Weapon == null || index == _CurWeapon) return;
            PutBackWeapon(_CurWeapon);
            TakeOutWeapon(index);
            _PreWeapon = _CurWeapon;
            _CurWeapon = index;
        }

        /// <summary>
        ///     切换为之前所持的武器
        /// </summary>
        public void SwitchWeaponToPre()
        {
            if (_PreWeapon == -1) return;
            SwitchWeapon(_PreWeapon);
        }

        /// <summary>
        ///     顺序查找第一个可以放入的单元格
        /// </summary>
        /// <param name="id"></param>
        /// <returns>是否成功放入</returns>
        public bool AddWeapon(ItemId id)
        {
            var info = _WeaponInfoDic[id];
            var type = info.Type;
            var index = -1;
            for (var i = 0; i < _Belt.Length; i++)
                if (_Belt[i].Type == type && _Belt[i].Weapon == null)
                {
                    index = i;
                    break;
                }

            if (index == -1) return false;

            var newWeapon = CreateWeapon(id);
            newWeapon.Trans.SetParent(_WeaponContainer);
            _Belt[index].Weapon = newWeapon;
            if (_CurWeapon == -1)
                SwitchWeapon(index);
            return true;
        }

        /// <summary>
        ///     设定指定位置的武器
        /// </summary>
        /// <param name="id"></param>
        /// <param name="offset">若有多个相同类型的槽位，offset指示第几个槽位，从0开始</param>
        /// <returns>被覆盖的武器id，-1为空或是未找到可用槽位</returns>
        public int SetWeapon(ItemId id, int offset = 0)
        {
            var info = _WeaponInfoDic[id];
            var index = 0;
            for (; index < _Belt.Length; index++)
                if (_Belt[index].Type == info.Type)
                {
                    offset--;
                    if (offset < 0)
                        break;
                }

            if (index == _Belt.Length) return -1;

            int result;
            if (_Belt[index].Weapon == null)
                result = -1;
            else
                result = _Belt[index].Weapon.ItemId;
            if (index == _CurWeapon) DestroyWeapon(index);
            _Belt[index].Weapon = CreateWeapon(id);
            return result;
        }

        /// <summary>
        ///     丢弃当前所持的武器
        /// </summary>
        public void DropCurrentWeapon()
        {
            if (_CurWeapon == -1) return;


            var index = _CurWeapon;
            var saveId = _Belt[_CurWeapon].Weapon.ItemId;
            saveId.Add = _Belt[_CurWeapon].Weapon.Drop();
            DestroyWeapon(_CurWeapon);
            if (_PreWeapon != -1)
            {
                TakeOutWeapon(_PreWeapon);
            }
            else
            {
                do
                {
                    index = (index + 1).Repeat(_Belt.Length);
                } while (_Belt[index].Weapon == null && index != _CurWeapon);

                if (index == _CurWeapon) index = -1;
            }

            _PreWeapon = -1;
            TakeOutWeapon(index);

            PublicEvents.DropItem?.Invoke(saveId);

            _CurWeapon = index;
        }

        /// <summary>
        ///     检测玩家输入，做出相应操作
        /// </summary>
        public void InputCheck()
        {
            if (ControlSys.Ins.RequireKey(GlobalSettings.PreWeaponKeyName, SHIELD_VALUE).Down)
                SwitchWeaponToPre();
            else
                foreach (var pairs in
                    _Keys.Where(pairs =>
                        ControlSys.Ins.RequireKey(pairs.Key, SHIELD_VALUE).Down))
                {
                    SwitchWeapon(pairs.Value);
                    break;
                }

            if (ControlSys.Ins.RequireKey(GlobalSettings.DropWeaponKeyName, SHIELD_VALUE).Down)
                DropCurrentWeapon();
        }

        /// <summary>
        /// 判断指定id的物品是不是武器
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsWeapon(ItemId id)
        {
            return _WeaponInfoDic.TryGetValue(id, out _);
        }

        private void TakeOutWeapon(int index)
        {
            if (index == _CurWeapon) return;
            var weapon = index == -1 ? _Unarmed : _Belt[index].Weapon;
            weapon.gameObject.SetActive(true);
            weapon.TakeOut();
        }

        private void PutBackWeapon(int index)
        {
            var weapon = index == -1 ? _Unarmed : _Belt[index].Weapon;
            weapon.PutBack();
            weapon.gameObject.SetActive(false);
        }

        private void DestroyWeapon(int index)
        {
            if (index == -1) return;

            if (_CurWeapon == index)
            {
                _Belt[index].Weapon.PutBack();
                _Belt[index].Weapon.gameObject.SetActive(false);
            }

            Object.Destroy(_Belt[index].Weapon.gameObject);
            _Belt[index].Weapon = null;
        }

        private BaseWeapon CreateWeapon(ItemId id)
        {
            var weapon =  ObjectPool.Ins.GetPrefab(_WeaponInfoDic[id].PrefabPath)
                .GetInstantiate()
                .GetComponent<BaseWeapon>();
            weapon.ReadAdditionalData(id.Add);
            weapon.ItemId = id;
            return weapon;
        }

        private static Dictionary<int, WeaponInfo> QueryDic()
        {
            var selected1 =
                from info in ItemSys.Ins.ItemInfos
                where (info.Flag & ItemFlag.Weapon) != 0
                select info;
            var selected2 =
                from row in DataBaseSys.Ins["weapon_info"]
                join info in selected1 on row["id"] equals info.Id.Id
                select new { Id = (int)info.Id, weaponInfo = new WeaponInfo(row["type"], info.WeaponPrefabPath)};
            return selected2.ToDictionary(x => x.Id, x => x.weaponInfo);
        }

        private readonly struct WeaponInfo
        {
            public readonly string Type;
            public readonly string PrefabPath;

            public WeaponInfo(string type, string prefabPath)
            {
                Type = type;
                PrefabPath = prefabPath;
            }
        }
    }

    public struct WeaponBeltCell
    {
        public string Type;
        public BaseWeapon Weapon;

        public WeaponBeltCell(string type, BaseWeapon weapon)
        {
            Type = type;
            Weapon = weapon;
        }
    }
}
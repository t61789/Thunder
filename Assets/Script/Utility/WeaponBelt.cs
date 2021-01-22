using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Thunder
{
    public class WeaponBelt : Package
    {
        public BaseWeapon CurrentWeapon => _CurWeapon == -1 ? _Unarmed : _Belt[_CurWeapon].Weapon;

        private const int SHIELD_VALUE = 0;

        private readonly WeaponBeltCell[] _Belt;
        private readonly WeaponBeltCell[] _TempBelt;
        private readonly Dictionary<string, int> _Keys;
        private readonly BaseWeapon _Unarmed;
        private readonly Transform _WeaponContainer;
        private int _CurWeapon = -1;
        private int _PreWeapon = -1;

        private static Dictionary<ItemId, WeaponInfo> _WeaponInfoDic;

        public WeaponBelt(Transform weaponContainer)
        {
            _Belt = InitBelt(Config.WeaponBeltTypes);
            _TempBelt = InitBelt(Config.WeaponBeltTypes);
            _Keys = BuildSwitchKeyMapping(Config.WeaponBeltTypes);
            _WeaponContainer = weaponContainer;
            if (_WeaponInfoDic == null)
                _WeaponInfoDic = QueryDic(DataBaseSys.GetTable(Config.WeaponInfoTableName));
            _Unarmed = CreateWeaponObj(Config.UnarmedId);
            _Unarmed.TakeOut();
        }

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
            _Belt[index].Weapon = CreateWeaponObj(id);
            _Belt[index].Weapon.TakeOut();
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
            saveId.Add = _Belt[_CurWeapon].Weapon.CompressItem();
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
            if (ControlSys.RequireKey(Config.PreWeaponKeyName, SHIELD_VALUE).Down)
                SwitchWeaponToPre();
            else
                foreach (var pairs in
                    _Keys.Where(pairs =>
                        ControlSys.RequireKey(pairs.Key, SHIELD_VALUE).Down))
                {
                    SwitchWeapon(pairs.Value);
                    break;
                }

            if (ControlSys.RequireKey(Config.DropWeaponKeyName, SHIELD_VALUE).Down)
                DropCurrentWeapon();
        }

        public void Destroy()
        {
            Object.Destroy(_Unarmed.gameObject);
            for (int i = 0; i < _Belt.Length; i++)
                DestroyWeapon(i);
        }

        /// <summary>
        /// 判断指定id的物品是不是武器
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsWeapon(ItemId id)
        {
            return _WeaponInfoDic.TryGetValue(id, out _);
        }

        private void TakeOutWeapon(int index)
        {
            var weapon = index == -1 ? _Unarmed : _Belt[index].Weapon;
            weapon.gameObject.SetActive(true);
            weapon.TakeOut();
            PublicEvents.TakeOutWeapon?.Invoke(weapon);
        }

        private void PutBackWeapon(int index)
        {
            var weapon = index == -1 ? _Unarmed : _Belt[index].Weapon;
            weapon.PutBack();
            weapon.gameObject.SetActive(false);
            PublicEvents.PutBackWeapon?.Invoke(weapon);
        }

        private void DestroyWeapon(int index)
        {
            if (_Belt[index].Group.Id == 0 || index == -1) return;

            if (_CurWeapon == index)
            {
                _Belt[index].Weapon.PutBack();
                _Belt[index].Weapon.gameObject.SetActive(false);
            }

            Object.Destroy(_Belt[index].Weapon.gameObject);
            _Belt[index].Weapon = null;
            _Belt[index].Group = new ItemGroup();
        }

        public override PackageOperation PutItem(ItemGroup itemGroup, bool putOnlySpaceEnough)
        {
            var result = PackageOperation.Take();
            result.RemainingNum = itemGroup.Count;

            if (!_WeaponInfoDic.TryGetValue(itemGroup.Id, out var info) ||
                itemGroup.Id != 0 && itemGroup.Count == 0)
                return result;


            var belt = _Belt;
            if (putOnlySpaceEnough)
            {
                for (int i = 0; i < _Belt.Length; i++)
                    _TempBelt[i] = _Belt[i];
                belt = _TempBelt;
            }

            var maxStack = ItemSys.GetInfo(itemGroup.Id).MaxStackNum;
            for (var i = 0; i < belt.Length; i++)
                if (belt[i].Type == info.Type &&
                    (belt[i].Group.Id == 0 || belt[i].Group.Id == itemGroup.Id))
                {
                    belt[i].Group.Id = itemGroup.Id;

                    var take = Mathf.Min(itemGroup.Count, maxStack - belt[i].Group.Count);
                    if (take == 0) continue;

                    belt[i].Group.Count += take;
                    result.RemainingNum -= take;

                    result.ItemChangeList.Add(i);

                    if (result.RemainingNum == 0)
                        break;
                }

            if (putOnlySpaceEnough && result.RemainingNum != 0)
            {
                if (result.RemainingNum != 0)
                {
                    result.RemainingNum = itemGroup.Count;
                    result.ItemChangeList.Clear();
                }
                else
                {
                    foreach (var i in result.ItemChangeList)
                        _Belt[i] = _TempBelt[i];
                    for (int i = 0; i < _TempBelt.Length; i++)
                        _TempBelt[i] = new WeaponBeltCell();
                }
            }

            foreach (var i in result.ItemChangeList)
            {
                if (_Belt[i].Weapon != null)
                    continue;

                _Belt[i].Weapon = CreateWeaponObj(itemGroup.Id);
                if (i == _CurWeapon)
                    _Belt[i].Weapon.TakeOut();
            }

            if (_CurWeapon == -1)
            {
                // todo 切换第一个可用武器
            }

            return result;
        }

        public override ItemGroup PutItemInto(int index, ItemGroup itemGroup)
        {
            var result = _Belt[index].Group;
            if (itemGroup.IsEmpty())
            {
                DestroyWeapon(index);
                return result;
            }

            var preId = result.Id;
            var maxStack = ItemSys.GetInfo(itemGroup.Id).MaxStackNum;

            // 源物品组数据非法不能添加
            // 交换操作无法完成不能添加
            // 源物品非武器不能添加
            // 目标单元格类型不正确不能添加
            if (itemGroup.IsInvalid() ||
                _Belt[index].Group.Id != 0 && _Belt[index].Group.Id != itemGroup.Id && itemGroup.Count > maxStack ||
                !_WeaponInfoDic.TryGetValue(itemGroup.Id, out var info) ||
                info.Type != _Belt[index].Type)
                return itemGroup;

            if (_Belt[index].Group.Id != itemGroup.Id)
            {
                _Belt[index].Group.Id = itemGroup.Id;
                _Belt[index].Group.Count = 0;
            }

            var take = Mathf.Min(maxStack - _Belt[index].Group.Count, itemGroup.Count);
            _Belt[index].Group.Count += take;

            if(itemGroup.Count > take)
                result.Id = itemGroup.Id;
            if (result.Id == 0 || result.Id == itemGroup.Id)
                result.Count = itemGroup.Count - take;

            if (preId != _Belt[index].Group.Id)
            {
                DestroyWeapon(index);
                _Belt[index].Weapon = CreateWeaponObj(_Belt[index].Group.Id);
                if(_CurWeapon==index)
                    TakeOutWeapon(index);
            }

            return result;
        }

        public override void SetItemAt(int index, ItemGroup itemGroup)
        {
            if (itemGroup.Count == 0)
            {
                if (itemGroup.Id != 0) return;

                DestroyWeapon(index);
                _Belt[index].Group = new ItemGroup();
                return;
            }

            if (!_WeaponInfoDic.TryGetValue(itemGroup.Id, out var info) ||
                info.Type != _Belt[index].Type)
                return;

            DestroyWeapon(index);
            _Belt[index].Group = itemGroup;
            _Belt[index].Weapon = CreateWeaponObj(itemGroup.Id);
            if (index == _CurWeapon)
                TakeOutWeapon(index);
        }

        public override IEnumerable<ItemGroup> GetAllItemInfo()
        {
            return from cell in _Belt
                select cell.Group;
        }

        public override ItemGroup GetItemInfoFrom(int index)
        {
            return _Belt[index].Group;
        }

        public override PackageOperation CostItem(ItemGroup itemGroup, bool costOnlyItemEnough)
        {
            var result = PackageOperation.Take();
            if (costOnlyItemEnough)
            {
                var count = 0;
                foreach (var weaponBowCell in _Belt)
                    if (weaponBowCell.Group.Id == itemGroup.Id)
                        count += weaponBowCell.Group.Count;
                if (count < itemGroup.Count)
                {
                    
                    result.RemainingNum = itemGroup.Count;
                    return result;
                }
            }

            result.RemainingNum = itemGroup.Count;
            for (int i = 0; i < _Belt.Length; i++)
            {
                if (_Belt[i].Group.Id != itemGroup.Id) continue;

                var take = Mathf.Min(_Belt[i].Group.Count, result.RemainingNum);
                _Belt[i].Group.Count -= take;
                result.RemainingNum -= take;

                result.ItemChangeList.Add(i);

                if (_Belt[i].Group.Count == 0)
                    DestroyWeapon(i);
                if (result.RemainingNum == 0)
                    break;
            }

            return result;
        }

        public override void SortItem()
        {
            throw new NotImplementedException();
        }

        private BaseWeapon CreateWeaponObj(ItemId id)
        {
            var weapon = ObjectPool.GetPrefab(_WeaponInfoDic[id].PrefabPath)
                .GetInstantiate()
                .GetComponent<BaseWeapon>();
            weapon.Init(_WeaponContainer,id.Add);
            weapon.gameObject.SetActive(false);
            return weapon;
        }

        private static Dictionary<ItemId, WeaponInfo> QueryDic(Table weaponTable)
        {
            var selected1 =
                from info in ItemSys.GetInfos()
                where info.IsWeapon
                select info;
            var selected2 =
                from row in weaponTable
                join info in selected1 on row["id"] equals info.Id.Id
                select new {info.Id, weaponInfo = new WeaponInfo((WeaponType)Enum.Parse(typeof(WeaponType), row["type"]), info.WeaponPrefabPath) };
            return selected2.ToDictionary(x => x.Id, x => x.weaponInfo);
        }

        private static Dictionary<string, int> BuildSwitchKeyMapping(WeaponType[] beltTypes)
        {
            var builder = new StringBuilder();
            const string switchStr = "Switch";
            var result = new Dictionary<string, int>();
            var preStr = (WeaponType)int.MaxValue;
            var repeatCount = 0;
            for (var i = 0; i < beltTypes.Length; i++)
            {
                builder.Clear();
                builder.Append(switchStr);
                var curType = beltTypes[i];

                if (curType == preStr)
                {
                    repeatCount++;
                }
                else
                {
                    repeatCount = 0;
                    preStr = curType;
                }

                builder.Append(curType);
                builder.Append(repeatCount);
                result.Add(builder.ToString(), i);
            }

            return result;
        }

        private static WeaponBeltCell[] InitBelt(WeaponType[] beltTypes)
        {
            var result = new WeaponBeltCell[beltTypes.Length];
            for (var i = 0; i < result.Length; i++)
                result[i].Type = beltTypes[i];
            return result;
        }

        private readonly struct WeaponInfo
        {
            public readonly WeaponType Type;
            public readonly string PrefabPath;

            public WeaponInfo(WeaponType type, string prefabPath)
            {
                Type = type;
                PrefabPath = prefabPath;
            }
        }
        public struct WeaponBeltCell
        {
            public WeaponType Type;
            public BaseWeapon Weapon;
            public ItemGroup Group;

            public WeaponBeltCell(WeaponType type, BaseWeapon weapon, ItemGroup group)
            {
                Type = type;
                Weapon = weapon;
                Group = group;
            }
        }
    }
}
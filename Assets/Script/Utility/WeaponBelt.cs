using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Thunder
{
    [Serializable]
    public class WeaponBelt : Package
    {
        public string DropWeaponKey = "DropWeapon";
        public string PreWeaponKey = "SwitchPreWeapon";

        public BaseWeapon CurrentWeapon => _CurWeapon == -1 ? _Unarmed : _Belt[_CurWeapon].Weapon;

        private const int SHIELD_VALUE = 0;

        private WeaponBeltCell[] _Belt;
        private Dictionary<string, int> _Keys;
        private BaseWeapon _Unarmed;
        private Transform _WeaponContainer;
        private int _CurWeapon = -1;
        private int _PreWeapon = -1;

        private static Dictionary<int, WeaponInfo> _WeaponInfoDic;

        private readonly Dictionary<ItemId, int> _ItemChangedDic
            = new Dictionary<ItemId, int>();
        private readonly List<int> _ItemCellChangedList
            = new List<int>();
        private readonly List<ItemGroup> _RemainingList
            = new List<ItemGroup>();

        public void Init(Transform weaponContainer)
        {
            _Belt = InitBelt(Config.WeaponBeltTypes);
            _Keys = BuildSwitchKeyMapping(Config.WeaponBeltTypes);
            _WeaponContainer = weaponContainer;
            _WeaponInfoDic = _WeaponInfoDic ?? QueryDic(DataBaseSys.GetTable(Config.WeaponInfoTableName));
            _Unarmed = CreateWeaponObj(Config.UnarmedId);
            _Unarmed.TakeOut();
        }

        /// <summary>
        /// 切换为目标单元格内的武器
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
        /// 切换为之前所持的武器
        /// </summary>
        public void SwitchWeaponToPre()
        {
            if (_PreWeapon == -1) return;
            SwitchWeapon(_PreWeapon);
        }

        /// <summary>
        /// 设定指定位置的武器
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
        /// 检测玩家输入，做出相应操作
        /// </summary>
        public void InputCheck()
        {
            if (ControlSys.RequireKey(CtrlKeys.GetKey(PreWeaponKey)).Down)
                SwitchWeaponToPre();
            else
                foreach (var pairs in
                    _Keys.Where(pairs =>
                        ControlSys.RequireKey(pairs.Key, SHIELD_VALUE).Down))
                {
                    SwitchWeapon(pairs.Value);
                    break;
                }

            if (ControlSys.RequireKey(CtrlKeys.GetKey(DropWeaponKey)).Down)
                DropCurrentWeapon();
        }

        public void Destroy()
        {
            Object.Destroy(_Unarmed.gameObject);
            for (int i = 0; i < _Belt.Length; i++)
                DestroyWeapon(i);
        }

        /// <summary>
        /// 丢弃当前所持的武器
        /// </summary>
        public void DropCurrentWeapon()
        {
            if (_CurWeapon == -1) return;

            var add = _Belt[_CurWeapon].Weapon.CompressItem();
            var id = _Belt[_CurWeapon].Group;
            id.Id.Add = add;
            DestroyWeapon(_CurWeapon);

            TakeOutWeapon(FindFirstAvailableWeapon(0));

            Player.Ins.Drop(id);
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
            _CurWeapon = index;
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

        public override int GetItemNum(ItemId id)
        {
            throw new NotImplementedException();
        }

        public override bool PutItemCheck(params ItemGroup[] groups)
        {
            _ItemChangedDic.Clear();
            _RemainingList.Clear();
            for (int i = 0; i < groups.Length; i++)
            {
                var g = groups[i].ToValid();
                if (!IsWeapon(g.Id)) return false;
                if (g.Id == 0) continue;
                _RemainingList.Add(g);
                _ItemChangedDic.Add(g.Id, _RemainingList.Count - 1);
            }

            if (_RemainingList.Count == 0)
                return true;

            var remainingCount = _RemainingList.Count;
            foreach (var c in _Belt)
            {
                var curCell = c;

                var curSrcItemIndex = FindAvailableGroup(_RemainingList, curCell.Type);
                if (curSrcItemIndex == -1) continue;
                if (curCell.Group.Id == 0)
                    curCell.Group.Id = _RemainingList[curSrcItemIndex].Id;

                if (!_ItemChangedDic.TryGetValue(curCell.Group.Id, out curSrcItemIndex))
                    continue;

                var maxStack = ItemSys.GetInfo(curCell.Group.Id).MaxStackNum;
                var g = _RemainingList[curSrcItemIndex];
                if (g.Count != 0)
                {
                    var take = Mathf.Min(g.Count, maxStack - curCell.Group.Count);
                    g.Count -= take;
                    _RemainingList[curSrcItemIndex] = g;
                    if (g.Count != 0) continue;

                    remainingCount --;
                    if (remainingCount <= 0)
                        break;
                }
            }


            return remainingCount <= 0;
        }

        public override PackageOperation PutItem(params ItemGroup[] groups)
        {
            _ItemChangedDic.Clear();
            _ItemCellChangedList.Clear();
            _RemainingList.Clear();
            for (int i = 0; i < groups.Length; i++)
            {
                var g = groups[i].ToValid();
                if (g.Id == 0 || !IsWeapon(g.Id)) continue;
                _RemainingList.Add(g);
                _ItemChangedDic.Add(g.Id, _RemainingList.Count - 1);
            }

            var result = new PackageOperation(
                JumpOverInvalidItem(_RemainingList),
                _ItemCellChangedList,
                ToChangedItemGroups(_ItemChangedDic));

            var remainingCount = _RemainingList.Count;
            for (int curCellIndex = 0; curCellIndex < _RemainingList.Count; curCellIndex++)
            {
                var curCell = _Belt[curCellIndex];

                var curSrcItemIndex = FindAvailableGroup(_RemainingList, curCell.Type);
                if (curSrcItemIndex == -1) continue;
                if (curCell.Group.Id == 0)
                    curCell.Group.Id = _RemainingList[curSrcItemIndex].Id;

                if (_ItemChangedDic.TryGetValue(curCell.Group.Id, out curSrcItemIndex))
                {
                    var maxStack = ItemSys.GetInfo(curCell.Group.Id).MaxStackNum;
                    var curSrcItem = _RemainingList[curSrcItemIndex];
                    if (curSrcItem.Count != 0)
                    {
                        var take = Mathf.Min(curSrcItem.Count, maxStack - curCell.Group.Count);
                        curSrcItem.Count -= take;
                        curCell.Group.Count += take;

                        if (take != 0)
                        {
                            _ItemCellChangedList.Add(curCellIndex);
                            _ItemChangedDic.ModifyIntDic(curCell.Group.Id, take);
                        }

                        _Belt[curCellIndex] = curCell;
                        _RemainingList[curSrcItemIndex] = curSrcItem;

                        if (curSrcItem.Count == 0)
                        {
                            remainingCount--;
                            if (remainingCount == 0)
                                break;
                        }
                    }
                }
            }

            foreach (var i in _ItemCellChangedList)
            {
                if (_Belt[i].Weapon != null)
                    continue;

                _Belt[i].Weapon = CreateWeaponObj(_Belt[i].Group.Id);
                if (i == _CurWeapon)
                    _Belt[i].Weapon.TakeOut();
            }

            if (_CurWeapon == -1)
                TakeOutWeapon(FindFirstAvailableWeapon(0));

            if(_ItemCellChangedList.Count!=0)
                InvokeOnItemCellChanged(result.PackageItemChangedInfo);
            return result;
        }

        public override bool CostItemCheck(params ItemGroup[] groups)
        {
            _ItemChangedDic.Clear();
            _RemainingList.Clear();
            for (int i = 0; i < groups.Length; i++)
            {
                var g = groups[i].ToValid();
                if (g.Id == 0) continue;
                _RemainingList.Add(g);
                _ItemChangedDic.Add(g.Id, _RemainingList.Count - 1);
            }

            var remainingCount = _RemainingList.Count;
            foreach (var curCell in _Belt)
            {
                if (_ItemChangedDic.TryGetValue(curCell.Group.Id,out var curSrcItemIndex))
                {
                    var curSrcItem = _RemainingList[curSrcItemIndex];
                    var take = Mathf.Min(curCell.Group.Count, curSrcItem.Count);
                    curSrcItem.Count -= take;
                    _RemainingList[curSrcItemIndex] = curSrcItem;
                    if (curSrcItem.Count == 0)
                    {
                        remainingCount--;
                        if (remainingCount == 0)
                            return true;
                    }
                }
            }

            return false;
        }

        public override PackageOperation CostItem(params ItemGroup[] groups)
        {
            _ItemChangedDic.Clear();
            _ItemCellChangedList.Clear();
            _RemainingList.Clear();
            for (int i = 0; i < groups.Length; i++)
            {
                var g = groups[i].ToValid();
                if (g.Id == 0 || !IsWeapon(g.Id)) continue;
                _RemainingList.Add(g);
                _ItemChangedDic.Add(g.Id, _RemainingList.Count - 1);
            }

            var result = new PackageOperation(
                JumpOverInvalidItem(_RemainingList), 
                _ItemCellChangedList, 
                ToChangedItemGroups(_ItemChangedDic));

            var remainingCount = _RemainingList.Count;
            for (int i = 0; i < _Belt.Length; i++)
            {
                var curCell = _Belt[i];
                if (_ItemChangedDic.TryGetValue(curCell.Group.Id, out var curSrcItemIndex))
                {
                    var curSrcItem = _RemainingList[curSrcItemIndex];
                    if(curSrcItem.Count==0)
                        continue;

                    var take = Mathf.Min(curCell.Group.Count, curSrcItem.Count);
                    curSrcItem.Count -= take;
                    curCell.Group.Count -= take;
                    _RemainingList[curSrcItemIndex] = curSrcItem;

                    if (curSrcItem.Count == 0)
                    {
                        remainingCount--;
                        if (remainingCount == 0)
                            break;
                    }

                    if (take != 0)
                    {
                        _ItemCellChangedList.Add(i);
                        _ItemChangedDic.ModifyIntDic(curSrcItem.Id,-take);
                    }
                }
            }

            foreach (var index in _ItemCellChangedList)
                if (_Belt[index].Group.Count == 0)
                    DestroyWeapon(index);

            if(_ItemCellChangedList.Count!=0)
                InvokeOnItemCellChanged(result.PackageItemChangedInfo);

            return result;
        }

        public override ItemGroup PutItemInto(int index, ItemGroup itemGroup)
        {
            itemGroup = itemGroup.ToValid();

            var result = _Belt[index].Group;
            if (itemGroup.IsEmpty())
            {
                DestroyWeapon(index);
                return result;
            }

            // 源物品非武器不能添加
            // 目标单元格类型不正确不能添加
            if (!_WeaponInfoDic.TryGetValue(itemGroup.Id, out var info) ||
                info.Type != _Belt[index].Type)
                return itemGroup;

            var preId = result.Id;
            var maxStack = ItemSys.GetInfo(itemGroup.Id).MaxStackNum;

            if (_Belt[index].Group.Id != 0)
                _Belt[index].Group.Id = itemGroup.Id;

            _ItemCellChangedList.Clear();
            _ItemChangedDic.Clear();
            if (_Belt[index].Group.Id != itemGroup.Id)
            {
                // 交换操作无法完成不能添加
                if (itemGroup.Count > maxStack)
                    return itemGroup;

                result = _Belt[index].Group;
                _ItemChangedDic.ModifyIntDic(result.Id,-result.Count);
                _Belt[index].Group = itemGroup;

                _ItemCellChangedList.Add(index);
            }
            else
            {
                var take = Mathf.Min(maxStack - _Belt[index].Group.Count, itemGroup.Count);
                _Belt[index].Group.Count += take;
                itemGroup.Count -= take;

                _ItemChangedDic.ModifyIntDic(itemGroup.Id,take);
                if(take!=0)
                    _ItemCellChangedList.Add(index);
            }

            if (preId != _Belt[index].Group.Id)
            {
                DestroyWeapon(index);
                _Belt[index].Weapon = CreateWeaponObj(_Belt[index].Group.Id);
                if (_CurWeapon == index)
                    TakeOutWeapon(index);
            }

            if(_ItemCellChangedList.Count!=0)
                InvokeOnItemCellChanged(
                    new PackageItemChangedInfo(
                        _ItemCellChangedList,
                        ToChangedItemGroups(_ItemChangedDic)));

            return result;
        }

        public override void SetItemAt(int index, ItemGroup itemGroup)
        {
            itemGroup = itemGroup.ToValid();

            _ItemCellChangedList.Clear();
            _ItemChangedDic.Clear();

            if (itemGroup.Id != 0 &&
                (!_WeaponInfoDic.TryGetValue(itemGroup.Id, out var info) ||
                 info.Type != _Belt[index].Type))
                return;

            if (!_Belt[index].Group.Equals(itemGroup))
                _ItemCellChangedList.Add(index);
            else
                return;

            if (_Belt[index].Group.Id != 0)
            {
                DestroyWeapon(index);
                var group = _Belt[index].Group;
                
                _ItemChangedDic.ModifyIntDic(group.Id,-group.Count);
            }

            _Belt[index].Group = itemGroup;
            if (itemGroup.Id != 0)
            {
                _ItemChangedDic.ModifyIntDic(itemGroup.Id, itemGroup.Count);
                _Belt[index].Weapon = CreateWeaponObj(_Belt[index].Group.Id);
            }
            if (index == _CurWeapon)
                TakeOutWeapon(index);

            if(_ItemCellChangedList.Count!=0)
                InvokeOnItemCellChanged(
                    new PackageItemChangedInfo(
                        _ItemCellChangedList,
                        ToChangedItemGroups(_ItemChangedDic)));
        }

        public override IEnumerable<ItemGroup> GetAllItemInfo()
        {
            return from cell in _Belt
                   select cell.Group;
        }

        public override ItemGroup GetItemInfo(int index)
        {
            return _Belt[index].Group;
        }

        public override void SortItem()
        {
            throw new NotImplementedException();
        }

        public string GetItemStr()
        {
            var sb = new StringBuilder();
            foreach (var weaponBeltCell in _Belt)
                sb.Append(weaponBeltCell.Group);

            return sb.ToString();
        }

        private BaseWeapon CreateWeaponObj(ItemId id)
        {
            var weapon = GameObjectPool.GetPrefab(_WeaponInfoDic[id].PrefabPath)
                .GetInstantiate()
                .GetComponent<BaseWeapon>();
            weapon.Init(_WeaponContainer, id.Add);
            weapon.gameObject.SetActive(false);
            return weapon;
        }

        private int FindFirstAvailableWeapon(int startIndex)
        {
            var i = startIndex;
            do
            {
                if (_Belt[i].Group.Id.Id == 0)
                {
                    i = (i + 1) % _Belt.Length;
                    continue;
                }

                return i;

            } while (i != startIndex);
            
            return -1;
        }

        private int FindAvailableGroup(IList<ItemGroup> groups, WeaponType weaponType)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].Count <= 0) continue;
                if (!_WeaponInfoDic.TryGetValue(groups[i].Id, out var info)) continue;
                if (info.Type == weaponType) return i;
            }

            return -1;
        }

        private static Dictionary<int, WeaponInfo> QueryDic(Table weaponTable)
        {
            var selected1 =
                from info in ItemSys.GetInfos()
                where info.IsWeapon
                select info;
            var selected2 =
                from row in weaponTable
                join info in selected1 on (int)row["id"].Data equals info.Id.Id
                select new { info.Id, weaponInfo = new WeaponInfo((WeaponType)Enum.Parse(typeof(WeaponType), row["type"]), info.WeaponPrefabPath) };
            return selected2.ToDictionary(x => x.Id.Id, x => x.weaponInfo);
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
        
        private static IEnumerable<ItemGroup> ToChangedItemGroups(Dictionary<ItemId, int> dic)
        {
            foreach (var i in dic)
                yield return new ItemGroup(i.Key, i.Value);
        }

        private static IEnumerable<ItemGroup> JumpOverInvalidItem(IEnumerable<ItemGroup> groups)
        {
            foreach (var itemGroup in groups)
            {
                if (itemGroup.Count != 0)
                    yield return itemGroup;
            }
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
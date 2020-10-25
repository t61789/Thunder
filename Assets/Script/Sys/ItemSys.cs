using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Thunder.Utility;
using Enum = System.Enum;

namespace Thunder.Sys
{
    public class ItemSys : IBaseSys, IEnumerable<KeyValuePair<int, ItemInfo>>
    {
        public static ItemSys Ins;

        private readonly Dictionary<int, ItemInfo> _ItemDic;

        public ItemInfo this[int id] => _ItemDic[id];

        public IEnumerable<int> ItemIds => _ItemDic.Keys;

        public IEnumerable<ItemInfo> ItemInfos => _ItemDic.Values;

        public ItemSys(Table itemInfoTable, ICollection<string> baseTypeName)
        {
            var pros = typeof(ItemInfo)
                .GetFields()
                .Where(x => baseTypeName.Contains(x.FieldType.Name))
                .ToArray();

            _ItemDic = new Dictionary<int, ItemInfo>();

            var flagType = typeof(ItemFlag);
            foreach (var row in itemInfoTable)
            {
                var info = new ItemInfo();
                foreach (var field in pros)
                    field.SetValue(info, row[field.Name].Data);

                //HandleSpecialData

                info.Id = (int)row["Id"];
                string flag = row["Flag"];
                if (!string.IsNullOrEmpty(flag))
                    info.Flag = (ItemFlag)Enum.Parse(flagType, flag);

                _ItemDic.Add(info.Id, info);
            }
        }

        public void OnSceneEnter(string preScene, string curScene)
        {
        }

        public void OnSceneExit(string curScene)
        {
        }

        public void OnApplicationExit()
        {
        }

        IEnumerator<KeyValuePair<int, ItemInfo>> IEnumerable<KeyValuePair<int, ItemInfo>>.GetEnumerator()
        {
            return _ItemDic.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return _ItemDic.GetEnumerator();
        }
    }

    public struct ItemId : IComparable<ItemId>
    {
        public int Id;
        public ItemAddData Add;

        public ItemId(int id, object add = null)
        {
            Id = id;
            Add = new ItemAddData(add);
        }

        public static implicit operator int(ItemId id)
        {
            return id.Id;
        }

        public static implicit operator ItemId(int id)
        {
            return new ItemId(id);
        }

        public static implicit operator (int id, object add)(ItemId id)
        {
            return (id.Id, id.Add);
        }

        public static implicit operator ItemId((int id, object add) id)
        {
            return new ItemId(id.Item1, id.Item2);
        }

        public int CompareTo(ItemId other)
        {
            if (other.Id > Id)
                return -1;
            return other.Id < Id ? 1 : 0;
        }
    }

    public readonly struct ItemAddData
    {
        private readonly object _Add;
        private static readonly LRUCache<string, object> _JsonCache =
            new LRUCache<string, object>(40);

        public ItemAddData(object additionData)
        {
            _Add = additionData;
        }

        /// <summary>
        /// 获取附加值，并转换为目标对象。若转换失败则尝试将其转为字符串并使用Json进行反序列化<br/><br/>
        /// note: 反序列化时，对于引用类型的附加值，每次都会进行反序列化获得新的对象。
        /// 而值类型的附加值则可以使用cache来提高速度，所以请尽量使用值类型来定义附加值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cast"></param>
        /// <returns></returns>
        public bool TryGet<T>(out T cast)
        {
            cast = default;
            if (_Add == null) return false;

            if (!(_Add is T))
            {
                if (!(_Add is string str)) return false;
                var valueType = cast is ValueType;
                if (valueType && _JsonCache.TryGet(str, out var obj))
                {
                    cast = (T)obj;
                    return true;
                }
                cast = JsonConvert.DeserializeObject<T>(str);
                if (valueType) _JsonCache.Add(str, cast);
                return true;
            }

            cast = (T)_Add;
            return true;
        }
    }

    public struct ItemGroup : IComparable<ItemGroup>
    {
        public ItemId Id;
        public int Count;

        private static readonly LRUCache<string, ItemGroup> _ItemGroupCache =
            new LRUCache<string, ItemGroup>(20);

        public ItemGroup(ItemId id, int count)
        {
            Id = id;
            Count = count;
        }

        public static implicit operator ItemGroup(ItemId id)
        {
            return new ItemGroup(id, 1);
        }

        public static implicit operator (ItemId id, int count)(ItemGroup itemGroup)
        {
            return (itemGroup.Id, itemGroup.Count);
        }

        public static implicit operator ItemGroup((ItemId id, int count) pair)
        {
            return new ItemGroup(pair.id, pair.count);
        }

        public static ItemGroup operator +(ItemGroup group, int count)
        {
            group.Count += count;
            return group;
        }

        public static ItemGroup operator +(int count, ItemGroup group)
        {
            group.Count += count;
            return group;
        }

        public int CompareTo(ItemGroup other)
        {
            var idComparison = Id.CompareTo(other.Id);
            return idComparison != 0 ? idComparison : Count.CompareTo(other.Count);
        }

        /// <summary>
        /// 格式：[(int) ItemId | (int) ItemNum | (string) AdditionDataByJson]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="itemGroup"></param>
        /// <returns></returns>
        public static bool TryParse(string str, out ItemGroup itemGroup)
        {
            if (_ItemGroupCache.TryGet(str, out itemGroup)) return true;

            var strs = str.Split('|');
            if (strs.Length != 3) return false;

            if (!int.TryParse(strs[0], out int id)) return false;

            string add = strs[2];
            if (string.IsNullOrEmpty(add)) add = null;

            var itemId = new ItemId(id, add);
            if (!int.TryParse(strs[1], out int count)) return false;

            itemGroup.Id = itemId;
            itemGroup.Count = count;

            _ItemGroupCache.Add(str,itemGroup);

            return true;
        }
    }

    public struct ItemInfo
    {
        public ItemId Id;
        public string Name;
        public ItemFlag Flag;
        public string PickPrefabPath;
        public string WeaponPrefabPath;
        public int MaxStackNum;
        public string PackageCellTexturePath;
    }
}

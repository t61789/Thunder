using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Framework;
using Newtonsoft.Json;
using Thunder;
using UnityEngine;
using Enum = System.Enum;

namespace Framework
{
    public class ItemSys : IBaseSys
    {
        public static ItemSys Ins;

        private static Dictionary<int, ItemInfo> _ItemInfo;

        public ItemSys()
        {
            Ins = this;
            _ItemInfo = new Dictionary<int, ItemInfo>();
            QueryDicFromTable(_ItemInfo,Config.ItemInfoTableName);
            QueryDicFromJson(_ItemInfo,Config.ItemInfoValuePath);
        }

        public static ItemInfo GetInfo(int id)
        {
            return _ItemInfo.TryGetAndException(id, $"未找到id为 {id} 的物品信息");
        }

        public static IEnumerable<ItemInfo> GetInfos()
        {
            return _ItemInfo.Values;
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

        private static void QueryDicFromTable(Dictionary<int, ItemInfo> dic,string tablePath)
        {
            var pros = typeof(ItemInfo)
                .GetProperties()
                .Where(x => DataBaseSys.AvailableDataType.Contains(x.PropertyType))
                .ToArray();

            const string idStr = "Id";
            ItemInfo defaultInfo = null;
            var stringType = typeof(string);
            foreach (var row in DataBaseSys.GetTable(tablePath))
            {
                var info = new ItemInfo();
                foreach (var field in pros)
                {
                    var data = row[field.Name].Data;

                    if (defaultInfo != null && (data == null || field == stringType && (data as string).IsNullOrEmpty()))
                    {
                        field.SetValue(info, field.GetValue(defaultInfo));
                        continue;
                    }

                    field.SetValue(info, data);
                }

                //HandleSpecialData
                info.Id = new ItemId(row[idStr]);

                if (defaultInfo == null)
                    defaultInfo = info;
                else
                    dic.Add(info.Id, info);
            }
        }

        private static void QueryDicFromJson(Dictionary<int, ItemInfo> dic, string valuePath)
        {
            var str = ValueSys.GetRawValue(valuePath);
            var reader = new JsonTextReader(new StringReader(str));
            var serializer = new JsonSerializer();
            ItemInfo defaultInfo = null;
            while (reader.Read())
            {
                if (defaultInfo == null)
                {
                    defaultInfo = serializer.Deserialize<ItemInfo>(reader);
                    continue;
                }

                var curInfo = defaultInfo.GetClone();
                serializer.Populate(reader, curInfo);
                dic.Add(curInfo.Id, curInfo);
            }
        }
    }
    
    [PreferenceAsset]
    public class ItemInfo
    {
        public ItemId Id {  set; get; }
        public string Name {  set; get; }
        public bool CanPackage {  set; get; }
        public int MaxStackNum {  set; get; }
        public bool IsWeapon {  set; get; }
        public string WeaponPrefabPath {  set; get; }
        public string PickPrefabPath {  set; get; }
        public string PackageCellTexturePath {  set; get; }

        public ItemInfo GetClone()
        {
            return (ItemInfo) this.MemberwiseClone();
        }
    }

    [Serializable]
    public struct ItemId : IComparable<ItemId>
    {
        public int Id;
        public string Add;

        public ItemId(int id, string add = null)
        {
            Id = id;
            Add = add;
        }

        public static implicit operator int(ItemId id)
        {
            return id.Id;
        }

        public static implicit operator ItemId(int id)
        {
            return new ItemId(id);
        }

        public static implicit operator (int id, string add)(ItemId id)
        {
            return (id.Id, id.Add);
        }

        public static implicit operator ItemId((int id, string add) id)
        {
            return new ItemId(id.id, id.add);
        }

        public int CompareTo(ItemId other)
        {
            if (other.Id > Id)
                return -1;
            return other.Id < Id ? 1 : 0;
        }

        public override string ToString()
        {
            return $"(Id:{Id} Add:{Add})";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ItemId)) return false;
            return Equals((ItemId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ (Add != null ? Add.GetHashCode() : 0);
            }
        }

        public bool Equals(ItemId other)
        {
            var bo = Add.IsNullOrEmpty() && other.Add.IsNullOrEmpty() ||
                     Add.Equals(other.Add);
            return Id == other.Id && bo;
        }

        //public int Id;
        //public ItemAddData Add;

        //public ItemId(int id, object add = null)
        //{
        //    Id = id;
        //    Add = new ItemAddData(add);
        //}

        //public static implicit operator int(ItemId id)
        //{
        //    return id.Id;
        //}

        //public static implicit operator ItemId(int id)
        //{
        //    return new ItemId(id);
        //}

        //public static implicit operator (int id, object add)(ItemId id)
        //{
        //    return (id.Id, id.Add);
        //}

        //public static implicit operator ItemId((int id, object add) id)
        //{
        //    return new ItemId(id.Item1, id.Item2);
        //}

        //public int CompareTo(ItemId other)
        //{
        //    if (other.Id > Id)
        //        return -1;
        //    return other.Id < Id ? 1 : 0;
        //}

        //public override string ToString()
        //{
        //    return $"(Id:{Id} Add:{Add})";
        //}
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

    [Serializable]
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

        public bool IsEmpty()
        {
            return Id == 0 && Count == 0;
        }

        /// <summary>
        /// 以下两种情况为不合法组：1.Id为0而数量不为0 2.数量为0而Id不为0
        /// </summary>
        /// <returns></returns>
        public bool IsInvalid()
        {
            return Id.Id == 0 && Count != 0 || Id != 0 && Count == 0;
        }

        /// <summary>
        /// 转换成合法组
        /// </summary>
        /// <returns></returns>
        public ItemGroup ToValid()
        {
            if (Id.Id == 0 || Count == 0)
                return new ItemGroup();
            return this;
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

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}

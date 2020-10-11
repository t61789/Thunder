﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Utility;

namespace Thunder.Sys
{
    public class ItemSys : IBaseSys,IEnumerable<KeyValuePair<int,ItemInfo>>
    {
        public static ItemSys Ins;

        private readonly Dictionary<int, ItemInfo> _ItemDic;

        public ItemInfo this[int id] => _ItemDic[id];

        public IEnumerable<int> ItemIds => _ItemDic.Keys;

        public IEnumerable<ItemInfo> ItemInfos => _ItemDic.Values;

        public ItemSys(DataBaseSys dataBase)
        {
            var pros = typeof(ItemInfo)
                .GetFields()
                .Where(x => DataBaseSys.AvaliableDataType.Contains(x.FieldType.Name))
                .ToArray();

            _ItemDic = new Dictionary<int, ItemInfo>();

            var flagType = typeof(ItemFlag);
            foreach (var row in DataBaseSys.Ins[GlobalSettings.ItemInfoTableName])
            {
                var info = new ItemInfo();
                foreach (var field in pros)
                    field.SetValue(info,row[field.Name].Data);

                //HandleSpecialData

                info.Id = (int)row["Id"];
                info.Flag = (ItemFlag) Enum.Parse(flagType, row["Flag"]);

                _ItemDic.Add(info.Id,info);
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

    public struct ItemId
    {
        public int Id;
        public object Add;

        public ItemId(int id, object add=null)
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

        public static implicit operator (int, object)(ItemId id)
        {
            return (id.Id,id.Add);
        }

        public static implicit operator ItemId((int, object) id)
        {
            return new ItemId(id.Item1,id.Item2);
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
    }
}

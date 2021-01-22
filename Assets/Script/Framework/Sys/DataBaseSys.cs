using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace Framework
{
    public class DataBaseSys : IBaseSys
    {
        public static readonly HashSet<Type> AvailableDataType = new HashSet<Type>()
        {
            typeof(int),
            typeof(string),
            typeof(bool),
            typeof(float)
        };
        private static readonly Dictionary<AssetId, TableUnit> _Tables = new Dictionary<AssetId, TableUnit>();

        /// <summary>
        /// 谨慎使用，若删除表后再获取会引起大量的重复载入
        /// </summary>
        public static bool DeleteTable(string tablePath)
        {
            return _Tables.Remove(AssetId.Parse(tablePath));
        }

        /// <summary>
        /// 删除指定bundle的所有表
        /// </summary>
        /// <param name="bundlePath"></param>
        public static void DeleteAllTable(string bundlePath)
        {
            var id = AssetId.Parse(bundlePath);
            var keys = _Tables.Keys.Where(x => x.Bundle == id.Bundle).ToArray();

            foreach (var tableId in keys)
                _Tables.Remove(tableId);
        }

        public static Table GetTable(string tablePath)
        {
            var id = AssetId.Parse(tablePath);
            for (var i = 0; i < 2; i++)
            {
                if (_Tables.TryGetValue(id, out var value) && string.IsNullOrEmpty(value.UnDeserialized)) return value.Deserialized;

                if (value.UnDeserialized != null)
                {
                    value.Deserialized =
                        new Table(JsonConvert.DeserializeObject<JsonTableReceiver>(value.UnDeserialized));
                    value.UnDeserialized = null;
                    _Tables[id] = value;
                    return value.Deserialized;
                }

                LoadBundle(id);

                if(!_Tables.TryGetValue(id, out _))
                    throw new Exception($"未在 {id.Bundle} 内找到名为 {id.Name} 的table");
            }

            return default;
        }

        public static void LoadBundle(string bundlePath)
        {
            LoadBundle(AssetId.Parse(bundlePath));
        }

        private static void LoadBundle(AssetId id)
        {
            var assets = BundleSys.GetAllAsset<TextAsset>(id.Bundle);

            foreach (var item in assets)
            {
                id.Name = item.name;
                if (_Tables.ContainsKey(id)) continue;
                _Tables.Add(id, new TableUnit(default, item.text));
            }

            BundleSys.ReleaseBundle(id.Bundle);
        }

        public void OnSceneEnter(string preScene, string curScene) { }

        public void OnSceneExit(string curScene) { }

        public void OnApplicationExit() { }

        private struct TableUnit
        {
            public Table Deserialized;
            public string UnDeserialized;

            public TableUnit(Table deserialized, string unDeserialized)
            {
                Deserialized = deserialized;
                UnDeserialized = unDeserialized;
            }
        }
    }

    public struct JsonTableReceiver
    {
        public string[] Fields;
        public object[][] Rows;
    }

    public class Table : IEnumerable<Row>
    {
        private readonly Dictionary<string, int> _FieldsDic;
        private readonly Row[] _Rows;

        public Table(JsonTableReceiver json)
        {
            Fields = json.Fields ?? new string[0];
            _FieldsDic = new Dictionary<string, int>();
            for (var i = 0; i < Fields.Length; i++)
                _FieldsDic.Add(Fields[i], i);

            _Rows = new Row[json.Rows.Length];
            for (var i = 0; i < json.Rows.Length; i++)
            {
                for (var j = 0; j < Fields.Length; j++)
                    switch (json.Rows[i][j])
                    {
                        case long _:
                            json.Rows[i][j] = (int)(long)json.Rows[i][j];
                            break;
                        case double _:
                            json.Rows[i][j] = (float)(double)json.Rows[i][j];
                            break;
                    }

                _Rows[i] = new Row(this, i, json.Rows[i]);
            }
        }

        public Row this[int index] => _Rows[index];

        public string[] Fields { get; }

        public IEnumerator<Row> GetEnumerator()
        {
            return ((IEnumerable<Row>)_Rows).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Rows.GetEnumerator();
        }

        public string GetField(int index)
        {
            return Fields[index];
        }

        public int GetField(string field)
        {
            return _FieldsDic[field];
        }

        public Row GetRow(int index)
        {
            return _Rows[index];
        }
    }

    public class Row : IEnumerable<Cell>
    {
        private readonly Cell[] _Cells;

        public Row(Table table, int index, IReadOnlyList<object> data)
        {
            Table = table;
            Index = index;
            _Cells = new Cell[data.Count];
            for (var i = 0; i < data.Count; i++)
                _Cells[i] = new Cell(Table, i, data[i]);
        }

        public Table Table { get; }
        public int Index { get; }

        public Cell this[int index] => _Cells[index];

        public Cell this[string field] => _Cells[Table.GetField(field)];

        public IEnumerator<Cell> GetEnumerator()
        {
            return ((IEnumerable<Cell>)_Cells).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Cells.GetEnumerator();
        }

        public Cell GetCell(int index)
        {
            return _Cells[index];
        }

        public Cell GetCell(string field)
        {
            return _Cells[Table.GetField(field)];
        }
    }

    public readonly struct Cell
    {
        public Table Table { get; }
        public object Data { get; }
        private readonly int _Index;

        public Cell(Table table, int index, object data)
        {
            Table = table;
            _Index = index;
            Data = data;
        }

        public string GetField()
        {
            return Table.GetField(_Index);
        }

        public static implicit operator int(Cell c)
        {
            return (int)c.Data;
        }

        public static implicit operator float(Cell c)
        {
            return (float)c.Data;
        }

        public static implicit operator string(Cell c)
        {
            return (string)c.Data;
        }

        public static implicit operator bool(Cell c)
        {
            return (bool)c.Data;
        }
    }
}
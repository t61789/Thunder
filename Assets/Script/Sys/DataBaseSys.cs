using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Sys
{
    #region oldcode

    //public class DataBaseSys : IBaseSys
    //{
    //    // 按照组(Group)读取，一个bundle内只包含一个dll和多张表
    //    // 每个组包含一个dll和一个bundle，bundle由多张表构成
    //    // 序列工具(Serializer)与组一一对应，dll的组织结构如下
    //    // Excel_Test
    //    //  string[] fields
    //    //  Test[] data
    //    // Test
    //    //  D1
    //    //  D2
    //    //  ..

    //    public static DataBaseSys Ins;

    //    private static readonly string DefaultBundle = Paths.DatabaseBundleD + Paths.Normal;

    //    private readonly Dictionary<AssetId, TableUnit> _Tables = new Dictionary<AssetId, TableUnit>();

    //    public DataTable this[string table] => GetTable(table);
    //    public DataTable this[string bundle, string table] => GetTable(null, bundle, table);
    //    public DataTable this[string bundleGroup, string bundle, string table] => GetTable(bundleGroup, bundle, table);

    //    public DataBaseSys()
    //    {
    //        Ins = this;
    //    }

    //    public DataTable GetTable(string tablePath)
    //    {
    //        return GetTable(AssetId.Create(tablePath, DefaultBundle));
    //    }

    //    public DataTable GetTable(string bundleGroup, string bundle, string name)
    //    {
    //        return GetTable(CreateId(bundleGroup, bundle, name));
    //    }

    //    /// <summary>
    //    /// 谨慎使用，若删除表后再获取会引起大量的重复载入
    //    /// </summary>
    //    public bool DeleteTable(string bundleGroup, string bundle, string name)
    //    {
    //        return DeleteTable(new AssetId(bundleGroup, bundle, name, DefaultBundle));
    //    }

    //    public bool DeleteTable(string tablePath)
    //    {
    //        return DeleteTable(AssetId.Create(tablePath, DefaultBundle));
    //    }

    //    private bool DeleteTable(AssetId id)
    //    {
    //        return _Tables.Dequeue(id);
    //    }

    //    /// <summary>
    //    /// 删除指定bundle的所有表
    //    /// </summary>
    //    /// <param name="bundleGroup"></param>
    //    /// <param name="bundle"></param>
    //    public void DeleteAllTable(string bundleGroup, string bundle)
    //    {
    //        DeleteAllTable(CreateId(bundleGroup, bundle, null));
    //    }

    //    private void DeleteAllTable(AssetId id)
    //    {
    //        var keys = _Tables.Keys.Where(x => x.Bundle == id.Bundle && x.BundleGroup == id.BundleGroup).ToArray();

    //        foreach (var tableId in keys)
    //            _Tables.Dequeue(tableId);
    //    }

    //    private static AssetId CreateId(string bundleGroup, string bundle, string name)
    //    {
    //        return new AssetId(bundleGroup, bundle, name, DefaultBundle);
    //    }

    //    #region json

    //    private struct TableUnit
    //    {
    //        public DataTable Deserialized;
    //        public string UnDeserialized;

    //        public TableUnit(DataTable deserialized, string unDeserialized)
    //        {
    //            Deserialized = deserialized;
    //            UnDeserialized = unDeserialized;
    //        }
    //    }

    //    private DataTable GetTable(AssetId id)
    //    {
    //        for (int i = 0; i < 2; i++)
    //        {
    //            if (_Tables.TryGetValue(id, out var value) && value.UnDeserialized == null) return value.Deserialized;

    //            if (value.UnDeserialized != null)
    //            {
    //                value.Deserialized = DataTable.LoadFromJsonReciever(
    //                    JsonConvert.DeserializeObject<DataTable.JsonReciever>(value.UnDeserialized));
    //                value.UnDeserialized = null;
    //                _Tables[id] = value;
    //                return value.Deserialized;
    //            }

    //            LoadBundle(id.BundleGroup, id.Bundle);

    //            Assert.IsTrue(_Tables.TryGetValue(id, out _), $"未在 {id.BundleGroup}!{id.Bundle} 内找到名为 {id.Name} 的table");
    //        }

    //        return default;
    //    }

    //    private void LoadBundle(string bundleGroup, string bundle)
    //    {
    //        var assets = BundleSys.Ins.GetAllAsset<TextAsset>(bundleGroup, bundle);

    //        foreach (var item in assets)
    //        {
    //            AssetId id = CreateId(bundleGroup, bundle, item.name);
    //            if (_Tables.ContainsKey(id)) continue;
    //            _Tables.Add(id, new TableUnit(default, item.text));
    //        }

    //        BundleSys.Ins.ReleaseBundle(bundleGroup, bundle);
    //    }

    //    #endregion

    //    #region protobuf

    //    //private readonly struct Serializer
    //    //{
    //    //    public readonly Type Ser;
    //    //    public readonly PropertyInfo[] FieldsInfo;

    //    //    public Serializer(Type ser, PropertyInfo[] fieldsInfo) : this()
    //    //    {
    //    //        Ser = ser;
    //    //        FieldsInfo = fieldsInfo;
    //    //    }
    //    //}

    //    //private struct TableUnit
    //    //{
    //    //    public DataTable Deserialized;
    //    //    public byte[] UnDeserialized;
    //    //    public readonly Serializer Deserializer;

    //    //    public TableUnit(DataTable deserialized, byte[] unDeserialized, Serializer deserializer)
    //    //    {
    //    //        Deserialized = deserialized;
    //    //        UnDeserialized = unDeserialized;
    //    //        Deserializer = deserializer;
    //    //    }
    //    //}

    //    //private readonly List<object[]> _TempRows = new List<object[]>();
    //    //private readonly List<object> _TempRow = new List<object>();
    //    //private readonly List<string> _TempFields = new List<string>();
    //    //private readonly Dictionary<AssetId, Serializer> _SerializersBuffer = new Dictionary<AssetId, Serializer>();

    //    //private DataTable GetTable(AssetId id)
    //    //{
    //    //    for (int i = 0; i < 2; i++)
    //    //    {
    //    //        if (_Tables.TryGetValue(id, out var value) && value.UnDeserialized == null) return value.Deserialized;

    //    //        if (value.UnDeserialized != null)
    //    //        {
    //    //            value.Deserialized = LoadFromProtobuf(value.Deserializer, value.UnDeserialized);
    //    //            value.UnDeserialized = null;
    //    //            _Tables[id] = value;
    //    //            return value.Deserialized;
    //    //        }

    //    //        LoadBundle(id.BundleGroup, id.Bundle);

    //    //        Assert.IsTrue(_Tables.TryGetValue(id, out _), $"未在 {id.BundleGroup}!{id.Bundle} 内找到名为 {id.Name} 的table");
    //    //    }

    //    //    return default;
    //    //}

    //    //private void LoadBundle(string bundleGroup, string bundle)
    //    //{
    //    //    _SerializersBuffer.Clear();

    //    //    const string ser = "serializer";

    //    //    var assets = BundleSys.Ins.GetAllAsset<TextAsset>(bundleGroup, bundle);
    //    //    var dllBytes = assets.FirstOrDefault(x => x.name == ser)?.bytes;
    //    //    Assert.IsNotNull(dllBytes, $"未在 {bundleGroup}!{bundle} 中找到解析dll");
    //    //    var serializers = Dll2Serializer(bundleGroup, bundle, dllBytes);

    //    //    foreach (var item in assets.Where(x => x.name != ser))
    //    //    {
    //    //        AssetId id = CreateId(bundleGroup, bundle, item.name);
    //    //        if (_Tables.ContainsKey(id)) continue;
    //    //        _Tables.Add(id, new TableUnit(default, item.bytes, serializers[id]));
    //    //    }

    //    //    BundleSys.Ins.ReleaseBundle(bundleGroup, bundle);
    //    //}

    //    //private DataTable LoadFromProtobuf(Serializer serializer, byte[] data)
    //    //{
    //    //    _TempRows.Clear();

    //    //    // 似乎在移动平台上不支持dynamic
    //    //    //dynamic temp = Ser.Ser.GetProperty("Parser").GetValue(null, null);
    //    //    //temp = temp.ParseFrom(data);
    //    //    //foreach (var item in temp.Data)
    //    //    //{
    //    //    //    _TempRow.Clear();
    //    //    //    foreach (var i in Ser.FieldsInfo)
    //    //    //        _TempRow.Add(i.GetValue(item, null));

    //    //    //    _TempRows.Add(_TempRow.ToArray());
    //    //    //}

    //    //    object parser = serializer.Ser.GetProperty("Parser")?.GetValue(null, null);
    //    //    object temp = parser?.GetType().GetMethod("ParseFrom", new[] { typeof(byte[]) })?.Invoke(parser, new object[] { data });

    //    //    //Type repeatedFieldType = typeof(Google.Protobuf.Collections.RepeatedField<>);
    //    //    parser = serializer.Ser.GetProperty("Data")?.GetValue(temp, null);  // 获取原始数据，由容器组成的数组
    //    //    IEnumerator enumerator = (IEnumerator)parser?.GetType().GetMethod("GetEnumerator")?.Invoke(parser, null); // 获取数组的迭代器

    //    //    while (enumerator != null && enumerator.MoveNext())
    //    //    {
    //    //        // 将容器类中的每个数据提取出来存入一行中

    //    //        object item = enumerator.Current;
    //    //        _TempRow.Clear();
    //    //        foreach (var i in serializer.FieldsInfo)
    //    //            _TempRow.Add(i.GetValue(item, null));

    //    //        _TempRows.Add(_TempRow.ToArray());
    //    //    }

    //    //    // 获取序列类中的field值
    //    //    parser = serializer.Ser.GetProperty("Fields")?.GetValue(temp, null);
    //    //    enumerator = (IEnumerator)parser?.GetType().GetMethod("GetEnumerator")?.Invoke(parser, null);

    //    //    _TempFields.Clear();
    //    //    while (enumerator != null && enumerator.MoveNext())
    //    //    {
    //    //        object item = enumerator.Current;
    //    //        _TempFields.Add(item as string);
    //    //    }

    //    //    //_TempFields.Clear();
    //    //    //foreach (var item in temp.Fields)
    //    //    //    _TempFields.Add(item);

    //    //    return new DataTable(_TempFields, _TempRows);
    //    //}

    //    //private readonly List<PropertyInfo> _FieldsInfoBuffer = new List<PropertyInfo>();

    //    //private Dictionary<AssetId, Serializer> Dll2Serializer(string bundleGroup, string bundle, byte[] bytes)
    //    //{
    //    //    Assembly assembly = Assembly.Load(bytes);
    //    //    string assemblyName = assembly.GetName().Name;
    //    //    const string s = "Excel_";
    //    //    _SerializersBuffer.Clear();

    //    //    foreach (var item in assembly.GetTypes())
    //    //    {
    //    //        if (!item.Name.StartsWith(s)) continue;

    //    //        string classname = item.Name.Replace(s, string.Empty);

    //    //        // 获取容器的属性信息
    //    //        Type container = assembly.GetType($"{assemblyName}.{classname}");

    //    //        _FieldsInfoBuffer.Clear();
    //    //        foreach (var prop in container.GetProperties(BindingFlags.Instance | BindingFlags.Public))
    //    //            _FieldsInfoBuffer.Add(prop);

    //    //        _SerializersBuffer.Add(CreateId(bundleGroup, bundle, classname), new Serializer(item, _FieldsInfoBuffer.ToArray()));
    //    //    }

    //    //    return _SerializersBuffer;
    //    //}

    //    #endregion

    //    #region xml
    //    //private const string ROWS = "rows";
    //    //private const string FIELDS = "fields";

    //    //private DataTable LoadFromXmlDoc(XDocument doc)
    //    //{
    //    //    fieldsBuff.Clear();

    //    //    foreach (var i in doc.Root.Element(FIELDS).Elements())
    //    //        fieldsBuff.Add(i.Name.ToString());

    //    //    temp_rows.Clear();
    //    //    foreach (var i in doc.Root.Element(ROWS).Elements())
    //    //    {
    //    //        temp_row.Clear();
    //    //        foreach (var j in i.Elements())
    //    //        {
    //    //            if (j.Value.Length == 0)
    //    //                temp_row.Add(null);
    //    //            else
    //    //                temp_row.Add(j.Value);
    //    //        }
    //    //        temp_rows.Add(temp_row.ToArray());
    //    //    }

    //    //    return new DataTable(fieldsBuff, temp_rows);
    //    //}
    //    #endregion

    //    public void OnSceneEnter(string preScene, string curScene)
    //    {

    //    }

    //    public void OnSceneExit(string curScene)
    //    {

    //    }

    //    public void OnApplicationExit()
    //    {

    //    }
    //}

    //#region datatable
    //public struct DataTable : IEnumerable<DataTable.Row>
    //{
    //    public bool IsEmpty;

    //    private readonly Dictionary<string, int> _Fields;

    //    public Row[] Rows { get; }

    //    public DataTable(IEnumerable<string> fields, IReadOnlyList<object[]> rows)
    //    {
    //        _Fields = new Dictionary<string, int>();
    //        var count = 0;
    //        foreach (var item in fields)
    //        {
    //            _Fields.Add(item, count);
    //            count++;
    //        }
    //        Rows = new Row[rows.Count];
    //        for (var i = 0; i < rows.Count; i++)
    //            Rows[i] = new Row(rows[i], _Fields);

    //        _Rows = null;
    //        _Row = null;
    //        _FieldsIndex = null;

    //        IsEmpty = rows.Count == 0;
    //    }

    //    public readonly struct Row
    //    {
    //        public object[] Cells { get; }
    //        private readonly Dictionary<string, int> _Fields;

    //        public Row(object[] cells, Dictionary<string, int> fields)
    //        {
    //            Cells = cells;
    //            _Fields = fields;
    //        }

    //        public object this[int index]
    //        {
    //            get
    //            {
    //                if (index < 0 || index >= Cells.Length)
    //                    return null;
    //                return Cells[index];
    //            }
    //        }

    //        public object this[string field] => _Fields.TryGetValue(field, out var value) ? Cells[value] : null;
    //    }

    //    public static DataTable LoadFromJsonReciever(JsonReciever jsonReciever)
    //    {
    //        int rank1 = jsonReciever.Rows.GetLength(0);
    //        int rank2 = jsonReciever.Fields.Length;
    //        for (int i = 0; i < rank1; i++)
    //            for (int j = 0; j < rank2; j++)
    //            {
    //                switch (jsonReciever.Rows[i][j])
    //                {
    //                    case long _:
    //                        jsonReciever.Rows[i][j] = (int)(long)jsonReciever.Rows[i][j];
    //                        break;
    //                    case double _:
    //                        jsonReciever.Rows[i][j] = (float)(double)jsonReciever.Rows[i][j];
    //                        break;
    //                }
    //            }
    //        return new DataTable(jsonReciever.Fields, (from row in jsonReciever.Rows select row).ToList());
    //    }

    //    public IEnumerable<string> Fields => _Fields.Keys;

    //    public int GetFieldIndex(string field)
    //    {
    //        return _Fields[field];
    //    }

    //    private List<object[]> _Rows;
    //    private List<object> _Row;
    //    private SortedList<int, string> _FieldsIndex;

    //    public DataTable Select(string[] fields = null, (string, object)[] where = null)
    //    {
    //        _Rows = _Rows ?? new List<object[]>();
    //        _FieldsIndex = _FieldsIndex ?? new SortedList<int, string>();
    //        _Row = _Row ?? new List<object>();

    //        _Rows.Clear();
    //        _FieldsIndex.Clear();

    //        if (fields != null)
    //        {
    //            foreach (var item in fields)
    //                _FieldsIndex.Add(GetFieldIndex(item), item);
    //        }
    //        else
    //        {
    //            var count = 0;
    //            foreach (var item in Fields)
    //            {
    //                _FieldsIndex.Add(count, item);
    //                count++;
    //            }
    //        }

    //        foreach (var item in Rows.Where(x =>
    //            @where == null || @where.All(i => x[i.Item1].Equals(i.Item2))))
    //        {
    //            _Row.Clear();
    //            foreach (var i in _FieldsIndex.Values)
    //                _Row.Add(item[i]);
    //            if (_Row.Count != 0)
    //                _Rows.Add(_Row.ToArray());
    //        }
    //        return new DataTable(_FieldsIndex.Values.ToArray(), _Rows);
    //    }

    //    public object SelectOnce(string field, (string, object)[] where = null)
    //    {
    //        _Rows = _Rows ?? new List<object[]>();
    //        _FieldsIndex = _FieldsIndex ?? new SortedList<int, string>();
    //        _Row = _Row ?? new List<object>();

    //        var row = Rows.FirstOrDefault(x =>
    //            @where == null || @where.All(i => x[i.Item1].Equals(i.Item2)));

    //        return row.Cells.Length == 0 ? null : row[_Fields[field]];
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return Rows.GetEnumerator();
    //    }

    //    IEnumerator<Row> IEnumerable<Row>.GetEnumerator()
    //    {
    //        return ((IEnumerable<Row>)Rows).GetEnumerator();
    //    }

    //    public struct JsonReciever
    //    {
    //        public string[] Fields;
    //        public object[][] Rows;
    //    }
    //}
    //#endregion

    #endregion

    public class DataBaseSys : IBaseSys
    {
        // 按照组(Group)读取，一个bundle内只包含一个dll和多张表
        // 每个组包含一个dll和一个bundle，bundle由多张表构成
        // 序列工具(Serializer)与组一一对应，dll的组织结构如下
        // Excel_Test
        //  string[] fields
        //  Test[] data
        // Test
        //  D1
        //  D2
        //  ..

        public static DataBaseSys Ins;

        private static readonly string _DefaultBundle = Paths.DatabaseBundleD + Paths.Normal;
        private readonly Dictionary<AssetId, TableUnit> _Tables = new Dictionary<AssetId, TableUnit>();

        public DataBaseSys()
        {
            Ins = this;
        }

        public Table this[string tablePath] => GetTable(tablePath);

        public void OnSceneEnter(string preScene, string curScene)
        {
        }

        public void OnSceneExit(string curScene)
        {
        }

        public void OnApplicationExit()
        {
        }

        /// <summary>
        ///     谨慎使用，若删除表后再获取会引起大量的重复载入
        /// </summary>
        public bool DeleteTable(string tablePath)
        {
            return _Tables.Remove(AssetId.Create(tablePath, _DefaultBundle));
        }

        /// <summary>
        ///     删除指定bundle的所有表
        /// </summary>
        /// <param name="bundlePath"></param>
        public void DeleteAllTable(string bundlePath)
        {
            var id = AssetId.Create(bundlePath, _DefaultBundle);
            var keys = _Tables.Keys.Where(x => x.Bundle == id.Bundle && x.BundleGroup == id.BundleGroup).ToArray();

            foreach (var tableId in keys)
                _Tables.Remove(tableId);
        }

        public Table GetTable(string tablePath)
        {
            var id = AssetId.Create(tablePath, _DefaultBundle);
            for (var i = 0; i < 2; i++)
            {
                if (_Tables.TryGetValue(id, out var value) && value.UnDeserialized == null) return value.Deserialized;

                if (value.UnDeserialized != null)
                {
                    value.Deserialized =
                        new Table(JsonConvert.DeserializeObject<JsonTableReciever>(value.UnDeserialized));
                    value.UnDeserialized = null;
                    _Tables[id] = value;
                    return value.Deserialized;
                }

                LoadBundle(id);

                Assert.IsTrue(_Tables.TryGetValue(id, out _),
                    $"未在 {id.BundleGroup}!{id.Bundle} 内找到名为 {id.Name} 的table");
            }

            return default;
        }

        public void LoadBundle(string bundlePath)
        {
            LoadBundle(AssetId.Create(bundlePath, _DefaultBundle));
        }

        private void LoadBundle(AssetId id)
        {
            var assets = BundleSys.Ins.GetAllAsset<TextAsset>(id.BundleGroup, id.Bundle);

            foreach (var item in assets)
            {
                id.Name = item.name;
                if (_Tables.ContainsKey(id)) continue;
                _Tables.Add(id, new TableUnit(default, item.text));
            }

            BundleSys.Ins.ReleaseBundle(id.BundleGroup, id.Bundle);
        }

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

    public struct JsonTableReciever
    {
        public string[] Fields;
        public object[][] Rows;
    }

    public class Table : IEnumerable<Row>
    {
        private readonly string[] _Fields;
        private readonly Dictionary<string, int> _FieldsDic;
        private readonly Row[] _Rows;

        public Table(JsonTableReciever json)
        {
            _Fields = json.Fields ?? new string[0];
            _FieldsDic = new Dictionary<string, int>();
            for (var i = 0; i < _Fields.Length; i++)
                _FieldsDic.Add(_Fields[i], i);

            _Rows = new Row[json.Rows.Length];
            for (var i = 0; i < json.Rows.Length; i++)
            {
                for (var j = 0; j < _Fields.Length; j++)
                    switch (json.Rows[i][j])
                    {
                        case long _:
                            json.Rows[i][j] = (int) (long) json.Rows[i][j];
                            break;
                        case double _:
                            json.Rows[i][j] = (float) (double) json.Rows[i][j];
                            break;
                    }

                _Rows[i] = new Row(this, i, json.Rows[i]);
            }
        }

        public Row this[int index] => _Rows[index];

        public IEnumerator<Row> GetEnumerator()
        {
            return ((IEnumerable<Row>) _Rows).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Rows.GetEnumerator();
        }

        public string GetField(int index)
        {
            return _Fields[index];
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
            return ((IEnumerable<Cell>) _Cells).GetEnumerator();
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
        private readonly int _Index;
        private readonly object _Data;

        public Cell(Table table, int index, object data)
        {
            Table = table;
            _Index = index;
            _Data = data;
        }

        public string GetField()
        {
            return Table.GetField(_Index);
        }

        public static implicit operator int(Cell c)
        {
            return (int) c._Data;
        }

        public static implicit operator float(Cell c)
        {
            return (float) c._Data;
        }

        public static implicit operator string(Cell c)
        {
            return (string) c._Data;
        }

        public static implicit operator bool(Cell c)
        {
            return (bool) c._Data;
        }
    }
}
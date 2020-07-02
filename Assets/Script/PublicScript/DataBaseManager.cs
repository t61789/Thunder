using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class DataBaseManager
{
    // 按照组(Group)读取
    // 每个组包含一个dll和一个bundle，bundle由多张表构成
    // 序列工具(Serializer)与组一一对应，dll的组织结构如下
    // Excel_Test
    //  string[] fields
    //  Test[] data
    // Test
    //  D1
    //  D2
    //  ..
    // 

    #region oldCode

    //private const string PROTOBUF_DLL = "ExcelDatabase";

    //private readonly Dictionary<string, byte[]> _UnDeserializedTables = new Dictionary<string, byte[]>();
    //private readonly Dictionary<string, DataTable> _Tables = new Dictionary<string, DataTable>();
    //private readonly Dictionary<string, Serializer> _Serializers = new Dictionary<string, Serializer>();

    //private readonly struct Serializer
    //{
    //    public readonly Type Ser;

    //    public readonly PropertyInfo[] FieldsInfo;

    //    public Serializer(Type ser, PropertyInfo[] fieldsInfo) : this()
    //    {
    //        Ser = ser;
    //        FieldsInfo = fieldsInfo;
    //    }
    //}

    //private readonly struct TableId
    //{
    //    public readonly string BundleGroup;
    //    public readonly string Bundle;
    //    public readonly string Name;

    //    public TableId(string bundleGroup, string bundle, string name)
    //    {
    //        BundleGroup = bundleGroup ?? Paths.BundleBasePath;
    //        Bundle = bundle ?? BundleManager.DatabaseBundle;
    //        Name = name;
    //    }
    //}

    //public DataTable this[string tablePath] => GetTable(tablePath);

    //public DataBaseManager()
    //{
    //    LoadAllSerializerDll();
    //}

    //private readonly List<PropertyInfo> _FieldsInfo = new List<PropertyInfo>();

    //private void LoadAllSerializerDll()
    //{
    //    Assembly assembly = Assembly.Load(PublicVar.bundle.GetAsset<TextAsset>(BundleManager.DllBundle, PROTOBUF_DLL).bytes);
    //    foreach (var item in assembly.GetTypes())
    //    {
    //        if (item.Name.Contains("Excel_"))
    //        {
    //            string classname = item.Name.Replace("Excel_", "");

    //            // 获取容器的属性信息
    //            Type container = assembly.GetType(PROTOBUF_DLL + "." + classname);

    //            _FieldsInfo.Clear();
    //            foreach (var prop in container.GetProperties(BindingFlags.Instance | BindingFlags.Public))
    //                _FieldsInfo.Add(prop);

    //            _Serializers.Add(classname, new Serializer(item, _FieldsInfo.ToArray()));
    //        }
    //    }
    //}

    //public DataTable GetTableNormal(string tableName)
    //{
    //    return GetTable(BundleManager.NormalD + tableName);
    //}

    //public DataTable GetTable(string tablePath)
    //{
    //    if (_Tables.TryGetValue(tablePath, out DataTable value))
    //        return value;

    //    if (_UnDeserializedTables.TryGetValue(tablePath, out byte[] bytes))
    //    {
    //        DataTable result = LoadFromProtobuf(_Serializers[tablePath], bytes);
    //        _UnDeserializedTables.Remove(tablePath);
    //        _Tables.Add(tablePath, result);
    //        return result;
    //    }

    //    int index = tablePath.LastIndexOf(Paths.Div);

    //    var bundlePath = index == -1 ? null : tablePath.Substring(0, index);

    //    foreach (var item in LoadBundle(bundlePath).Where(item => !_UnDeserializedTables.TryGetValue(item.Item1, out _)))
    //    {
    //        _UnDeserializedTables.Add(item.Item1, item.Item2);
    //    }

    //    if (!_UnDeserializedTables.TryGetValue(tablePath, out _))
    //    {
    //        Debug.LogError("No such table named " + tablePath);
    //        throw new Exception();
    //    }

    //    return GetTable(tablePath);
    //}

    //private List<(string, byte[])> LoadBundle(string bundlePath)
    //{
    //    List<(string, byte[])> result = new List<(string, byte[])>();

    //    StringBuilder pathBuilder = new StringBuilder();

    //    string temp;
    //    if (bundlePath == null)
    //        temp = BundleManager.DatabaseBundleD + BundleManager.Normal;
    //    else
    //        temp = BundleManager.DatabaseBundleD + bundlePath;

    //    foreach (var item in PublicVar.bundle.GetAllAsset<TextAsset>(temp))
    //    {
    //        pathBuilder.Clear();
    //        if (bundlePath != null)
    //        {
    //            pathBuilder.Append(bundlePath);
    //            pathBuilder.Append(Paths.Div);
    //        }
    //        pathBuilder.Append(item.name);
    //        result.Add((pathBuilder.ToString(), item.bytes));
    //    }

    //    PublicVar.bundle.ReleaseBundle(temp);

    //    return result;
    //}

    //public bool DeleteTable(string tablePath)
    //{
    //    return _Tables.Remove(tablePath);
    //}

    //private readonly List<object[]> _TempRows = new List<object[]>();
    //private readonly List<object> _TempRow = new List<object>();
    //private readonly List<string> _TempFields = new List<string>();

    //#region xml
    ////private const string ROWS = "rows";
    ////private const string FIELDS = "fields";

    ////private DataTable LoadFromXmlDoc(XDocument doc)
    ////{
    ////    fieldsBuff.Clear();

    ////    foreach (var i in doc.Root.Element(FIELDS).Elements())
    ////        fieldsBuff.Add(i.Name.ToString());

    ////    temp_rows.Clear();
    ////    foreach (var i in doc.Root.Element(ROWS).Elements())
    ////    {
    ////        temp_row.Clear();
    ////        foreach (var j in i.Elements())
    ////        {
    ////            if (j.Value.Length == 0)
    ////                temp_row.Add(null);
    ////            else
    ////                temp_row.Add(j.Value);
    ////        }
    ////        temp_rows.Add(temp_row.ToArray());
    ////    }

    ////    return new DataTable(fieldsBuff, temp_rows);
    ////}
    //#endregion


    //private DataTable LoadFromProtobuf(Serializer serializer, byte[] data)
    //{
    //    _TempRows.Clear();

    //    // 似乎在移动平台上不支持dynamic
    //    //dynamic temp = Ser.Ser.GetProperty("Parser").GetValue(null, null);
    //    //temp = temp.ParseFrom(data);
    //    //foreach (var item in temp.Data)
    //    //{
    //    //    _TempRow.Clear();
    //    //    foreach (var i in Ser.FieldsInfo)
    //    //        _TempRow.Add(i.GetValue(item, null));

    //    //    _TempRows.Add(_TempRow.ToArray());
    //    //}

    //    object parser = serializer.Ser.GetProperty("Parser")?.GetValue(null, null);
    //    object temp = parser?.GetType().GetMethod("ParseFrom", new[] { typeof(byte[]) })?.Invoke(parser, new object[] { data });

    //    //Type repeatedFieldType = typeof(Google.Protobuf.Collections.RepeatedField<>);
    //    parser = serializer.Ser.GetProperty("Data")?.GetValue(temp, null);  // 获取原始数据，由容器组成的数组
    //    System.Collections.IEnumerator enumerator = (System.Collections.IEnumerator)parser?.GetType().GetMethod("GetEnumerator")?.Invoke(parser, null); // 获取数组的迭代器

    //    while (enumerator != null && enumerator.MoveNext())
    //    {
    //        // 将容器类中的每个数据提取出来存入一行中

    //        object item = enumerator.Current;
    //        _TempRow.Clear();
    //        foreach (var i in serializer.FieldsInfo)
    //            _TempRow.Add(i.GetValue(item, null));

    //        _TempRows.Add(_TempRow.ToArray());
    //    }

    //    // 获取序列类中的field值
    //    parser = serializer.Ser.GetProperty("Fields")?.GetValue(temp, null);
    //    enumerator = (System.Collections.IEnumerator)parser?.GetType().GetMethod("GetEnumerator")?.Invoke(parser, null);

    //    _TempFields.Clear();
    //    while (enumerator != null && enumerator.MoveNext())
    //    {
    //        object item = enumerator.Current;
    //        _TempFields.Add(item as string);
    //    }

    //    //_TempFields.Clear();
    //    //foreach (var item in temp.Fields)
    //    //    _TempFields.Add(item);

    //    return new DataTable(_TempFields, _TempRows);
    //}

    #endregion


    private readonly Dictionary<TableId, TableUnit> _Tables = new Dictionary<TableId, TableUnit>();

    private readonly struct Serializer
    {
        public readonly Type Ser;
        public readonly PropertyInfo[] FieldsInfo;

        public Serializer(Type ser, PropertyInfo[] fieldsInfo) : this()
        {
            Ser = ser;
            FieldsInfo = fieldsInfo;
        }
    }

    private struct TableId
    {
        private readonly string _BundleGroup;
        private readonly string _Bundle;
        public string Name;

        public TableId(string bundleGroup, string bundle, string name)
        {
            _BundleGroup = Path.GetFullPath(bundleGroup ?? Paths.BundleBasePath);
            _Bundle = bundle ?? BundleManager.DatabaseBundle;
            Name = name;
        }
    }

    private struct TableUnit
    {
        public DataTable Deserialized;
        public byte[] UnDeserialized;
        public readonly Serializer Deserializer;

        public TableUnit(DataTable deserialized,byte[] unDeserialized, Serializer deserializer)
        {
            Deserialized = deserialized;
            UnDeserialized = unDeserialized;
            Deserializer = deserializer;
        }
    }

    public DataTable this[string table] => GetTable(null,null,table);
    public DataTable this[string bundle,string table] => GetTable(null, bundle, table);
    public DataTable this[string bundleGroup,string bundle, string table] => GetTable(bundleGroup, bundle, table);

    public DataTable GetTable(string bundleGroup, string bundle, string name)
    {
        TableId id = new TableId(bundleGroup, bundle, name);

        for (int i = 0; i < 2; i++)
        {
            if (_Tables.TryGetValue(id, out var value) && value.UnDeserialized == null) return value.Deserialized;

            if (value.UnDeserialized != null)
            {
                value.Deserialized = LoadFromProtobuf(value.Deserializer, value.UnDeserialized);
                value.UnDeserialized = null;
                _Tables[id] = value;
                return value.Deserialized;
            }

            LoadBundle(bundleGroup, bundle);

            id.Name = name;
            Assert.IsTrue(_Tables.TryGetValue(id, out _), "未在bundleGroup " + bundleGroup + " 的bundle " + bundle + " 内找到名为 " + name + " 的table");
        }

        return default;
    }

    private readonly Dictionary<TableId, Serializer> _SerializersBuffer = new Dictionary<TableId, Serializer>();

    private void LoadBundle(string bundleGroup, string bundle)
    {
        _SerializersBuffer.Clear();

        const string ser = "serializer";

        TextAsset[] assets = PublicVar.bundle.GetAllAsset<TextAsset>(bundleGroup, bundle);
        byte[] dllBytes = assets.First(x => x.name == ser)?.bytes;
        Assert.IsNotNull(dllBytes,$"未在bundleGroup {bundleGroup} 的bundle {bundle} 中找到解析dll");
        var serializers = Dll2Serializer(bundleGroup, bundle, dllBytes);

        foreach (var item in assets.Where(x => x.name != ser))
        {
            TableId id = new TableId(bundleGroup,bundle,item.name);
            _Tables.Add(id, new TableUnit(default,item.bytes, serializers[id]));
        }
        
        PublicVar.bundle.ReleaseBundle(bundleGroup,bundle);
    }

    private readonly List<PropertyInfo> _FieldsInfoBuffer = new List<PropertyInfo>();

    private Dictionary<TableId, Serializer> Dll2Serializer(string bundleGroup, string bundle,byte[] bytes)
    {
        Assembly assembly = Assembly.Load(bytes);
        string assemblyName = assembly.GetName().Name;
        const string s = "Excel_";
        _SerializersBuffer.Clear();

        foreach (var item in assembly.GetTypes())
        {
            if (!item.Name.StartsWith(s)) continue;

            string classname = item.Name.Replace(s, string.Empty);

            // 获取容器的属性信息
            Type container = assembly.GetType($"{assemblyName}.{classname}");

            _FieldsInfoBuffer.Clear();
            foreach (var prop in container.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                _FieldsInfoBuffer.Add(prop);

            _SerializersBuffer.Add(new TableId(bundleGroup,bundle,classname), new Serializer(item, _FieldsInfoBuffer.ToArray()));
        }

        return _SerializersBuffer;
    }

    private readonly List<object[]> _TempRows = new List<object[]>();
    private readonly List<object> _TempRow = new List<object>();
    private readonly List<string> _TempFields = new List<string>();

    #region xml
    //private const string ROWS = "rows";
    //private const string FIELDS = "fields";

    //private DataTable LoadFromXmlDoc(XDocument doc)
    //{
    //    fieldsBuff.Clear();

    //    foreach (var i in doc.Root.Element(FIELDS).Elements())
    //        fieldsBuff.Add(i.Name.ToString());

    //    temp_rows.Clear();
    //    foreach (var i in doc.Root.Element(ROWS).Elements())
    //    {
    //        temp_row.Clear();
    //        foreach (var j in i.Elements())
    //        {
    //            if (j.Value.Length == 0)
    //                temp_row.Add(null);
    //            else
    //                temp_row.Add(j.Value);
    //        }
    //        temp_rows.Add(temp_row.ToArray());
    //    }

    //    return new DataTable(fieldsBuff, temp_rows);
    //}
    #endregion

    private DataTable LoadFromProtobuf(Serializer serializer, byte[] data)
    {
        _TempRows.Clear();

        // 似乎在移动平台上不支持dynamic
        //dynamic temp = Ser.Ser.GetProperty("Parser").GetValue(null, null);
        //temp = temp.ParseFrom(data);
        //foreach (var item in temp.Data)
        //{
        //    _TempRow.Clear();
        //    foreach (var i in Ser.FieldsInfo)
        //        _TempRow.Add(i.GetValue(item, null));

        //    _TempRows.Add(_TempRow.ToArray());
        //}

        object parser = serializer.Ser.GetProperty("Parser")?.GetValue(null, null);
        object temp = parser?.GetType().GetMethod("ParseFrom", new[] { typeof(byte[]) })?.Invoke(parser, new object[] { data });

        //Type repeatedFieldType = typeof(Google.Protobuf.Collections.RepeatedField<>);
        parser = serializer.Ser.GetProperty("Data")?.GetValue(temp, null);  // 获取原始数据，由容器组成的数组
        System.Collections.IEnumerator enumerator = (System.Collections.IEnumerator)parser?.GetType().GetMethod("GetEnumerator")?.Invoke(parser, null); // 获取数组的迭代器

        while (enumerator != null && enumerator.MoveNext())
        {
            // 将容器类中的每个数据提取出来存入一行中

            object item = enumerator.Current;
            _TempRow.Clear();
            foreach (var i in serializer.FieldsInfo)
                _TempRow.Add(i.GetValue(item, null));

            _TempRows.Add(_TempRow.ToArray());
        }

        // 获取序列类中的field值
        parser = serializer.Ser.GetProperty("Fields")?.GetValue(temp, null);
        enumerator = (System.Collections.IEnumerator)parser?.GetType().GetMethod("GetEnumerator")?.Invoke(parser, null);

        _TempFields.Clear();
        while (enumerator != null && enumerator.MoveNext())
        {
            object item = enumerator.Current;
            _TempFields.Add(item as string);
        }

        //_TempFields.Clear();
        //foreach (var item in temp.Fields)
        //    _TempFields.Add(item);

        return new DataTable(_TempFields, _TempRows);
    }
}

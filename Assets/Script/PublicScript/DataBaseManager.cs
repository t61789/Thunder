using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

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


    private const string PROTOBUF_DLL = "ExcelDatabase";

    private readonly Dictionary<string, byte[]> _UnDeserializedTables = new Dictionary<string, byte[]>();
    private readonly Dictionary<string, DataTable> _Tables = new Dictionary<string, DataTable>();
    private readonly Dictionary<string, Serializer> _Serializers = new Dictionary<string, Serializer>();

    private struct Serializer
    {
        public readonly Type Con;
        public readonly Type Ser;

        public PropertyInfo[] FieldsInfo;

        public Serializer(Type con, Type ser) : this()
        {
            Con = con;
            Ser = ser;
        }
    }

    public DataTable this[string tablePath] => GetTable(tablePath);

    public DataBaseManager()
    {
        LoadAllSerializerDll();
    }

    private void LoadAllSerializerDll()
    {
        Assembly assembly = Assembly.Load(PublicVar.bundle.GetAsset<TextAsset>(BundleManager.DllBundle, PROTOBUF_DLL).bytes);
        foreach (var item in assembly.GetTypes())
        {
            if (item.Name.Contains("Excel_"))
            {
                string classname = item.Name.Replace("Excel_", "");
                _Serializers.Add(classname, new Serializer(assembly.GetType(PROTOBUF_DLL + "." + classname), item));
            }
        }
    }

    public DataTable GetTableNormal(string tableName)
    {
        return GetTable(BundleManager.NormalD + tableName);
    }

    public DataTable GetTable(string tablePath)
    {
        if (_Tables.TryGetValue(tablePath, out DataTable value))
            return value;

        if (_UnDeserializedTables.TryGetValue(tablePath, out byte[] bytes))
        {
            DataTable result = LoadFromProtobuf(_Serializers[tablePath], bytes);
            _UnDeserializedTables.Remove(tablePath);
            _Tables.Add(tablePath, result);
            return result;
        }

        int index = tablePath.LastIndexOf(Paths.Div);

        var bundlePath = index == -1 ? null : tablePath.Substring(0, index);

        foreach (var item in LoadBundle(bundlePath).Where(item => !_UnDeserializedTables.TryGetValue(item.Item1, out _)))
        {
            _UnDeserializedTables.Add(item.Item1, item.Item2);
        }

        if (!_UnDeserializedTables.TryGetValue(tablePath, out _))
        {
            Debug.LogError("No such table named " + tablePath);
            throw new Exception();
        }

        return GetTable(tablePath);
    }

    private List<(string, byte[])> LoadBundle(string bundlePath)
    {
        List<(string, byte[])> result = new List<(string, byte[])>();

        StringBuilder pathBuilder = new StringBuilder();

        string temp;
        if (bundlePath == null)
            temp = BundleManager.DatabaseBundleD + BundleManager.Normal;
        else
            temp = BundleManager.DatabaseBundleD + bundlePath;

        foreach (var item in PublicVar.bundle.GetAllAsset<TextAsset>(temp))
        {
            pathBuilder.Clear();
            if (bundlePath != null)
            {
                pathBuilder.Append(bundlePath);
                pathBuilder.Append(Paths.Div);
            }
            pathBuilder.Append(item.name);
            result.Add((pathBuilder.ToString(), item.bytes));
        }

        PublicVar.bundle.ReleaseBundle(temp);

        return result;
    }

    public bool DeleteTable(string tablePath)
    {
        return _Tables.Remove(tablePath);
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

    private readonly List<PropertyInfo> _FieldsInfo = new List<PropertyInfo>();

    private DataTable LoadFromProtobuf(Serializer serializer, byte[] data)
    {
        if (serializer.FieldsInfo == null)
        {
            _FieldsInfo.Clear();
            foreach (var item in serializer.Con.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                _FieldsInfo.Add(item);
            serializer.FieldsInfo = _FieldsInfo.ToArray();
        }

        _TempRows.Clear();
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
        parser = serializer.Ser.GetProperty("Data")?.GetValue(temp, null);
        System.Collections.IEnumerator enumerator = (System.Collections.IEnumerator)parser?.GetType().GetMethod("GetEnumerator")?.Invoke(parser, null);

        while (enumerator != null && enumerator.MoveNext())
        {
            object item = enumerator.Current;
            _TempRow.Clear();
            foreach (var i in serializer.FieldsInfo)
                _TempRow.Add(i.GetValue(item, null));

            _TempRows.Add(_TempRow.ToArray());
        }

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

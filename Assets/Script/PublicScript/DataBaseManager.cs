using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using Google.Protobuf;
using Tool;
using System.Text;

public class DataBaseManager
{
    private const string PROTOBUF_DLL = "ExcelDatabase";

    private readonly Dictionary<string, byte[]> unDeserializedTables = new Dictionary<string, byte[]>();
    private readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();
    private readonly Dictionary<string, Serializer> serializers = new Dictionary<string, Serializer>();

    private struct Serializer
    {
        public Type container;
        public Type serializer;

        public PropertyInfo[] fieldsInfo;

        public Serializer(Type container, Type serializer) : this()
        {
            this.container = container;
            this.serializer = serializer;
        }
    }

    public DataTable this[string tablePath]
    {
        get
        {
            return GetTable(tablePath);
        }
    }

    public DataTable GetTableNormal(string tableName)
    {
        return GetTable(BundleManager.NormalD+tableName);
    }

    public DataTable GetTable(string tablePath)
    {
        if (tables.TryGetValue(tablePath, out DataTable value))
            return value;

        if (unDeserializedTables.TryGetValue(tablePath, out byte[] bytes))
        {
            DataTable result = LoadFromProtobuf(serializers[tablePath],bytes);
            unDeserializedTables.Remove(tablePath);
            tables.Add(tablePath, result);
            return result;
        }

        int index = tablePath.LastIndexOf(BundleManager.PathDivider);

        foreach (var item in LoadBundle(tablePath.Substring(0, index)))
        {
            if (!unDeserializedTables.TryGetValue(item.Item1, out _))
                unDeserializedTables.Add(item.Item1, item.Item2);
        }

        if(!unDeserializedTables.TryGetValue(tablePath,out _))
        {
            Debug.LogError("No such table named "+tablePath);
            throw new Exception();
        }

        return GetTable(tablePath);
    }

    private List<(string, byte[])> LoadBundle(string bundlePath)
    {
        List<(string, byte[])> result = new List<(string, byte[])>();

        StringBuilder pathBuilder = new StringBuilder();
        foreach (var item in PublicVar.bundleManager.GetAllAsset<TextAsset>(bundlePath))
        {
            pathBuilder.Clear();
            pathBuilder.Append(BundleManager.BundleBasePath);
            pathBuilder.Append(bundlePath);
            pathBuilder.Append(BundleManager.PathDivider);
            pathBuilder.Append(item.name);
            result.Add((pathBuilder.ToString(), item.bytes));
        }

        PublicVar.bundleManager.ReleaseBundle(bundlePath);

        return result;
    }

    public DataBaseManager()
    {
        LoadAllSerializerDll();
    }

    private void LoadAllSerializerDll()
    {
        Assembly assembly = Assembly.Load(PublicVar.bundleManager.GetAsset<TextAsset>(BundleManager.DllBundle, PROTOBUF_DLL).bytes);
        foreach (var item in assembly.GetTypes())
        {
            if (item.Name.Contains("Excel_"))
            {
                string classname = item.Name.Replace("Excel_", "");
                serializers.Add(classname, new Serializer(assembly.GetType(PROTOBUF_DLL + "." + classname), item));
            }
        }
    }

    public bool DeleteTable(string tablePath)
    {
        return tables.Remove(tablePath);
    }

    private readonly List<object[]> tempRows = new List<object[]>();
    private readonly List<object> tempRow = new List<object>();
    private readonly List<string> tempFields = new List<string>();

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

    private readonly List<PropertyInfo> fieldsInfo = new List<PropertyInfo>();

    private DataTable LoadFromProtobuf(Serializer serializer, byte[] data)
    {
        if (serializer.fieldsInfo == null)
        {
            fieldsInfo.Clear();
            foreach (var item in serializer.container.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                fieldsInfo.Add(item);
            serializer.fieldsInfo = fieldsInfo.ToArray();
        }

        tempRows.Clear();
        dynamic temp = serializer.serializer.GetProperty("Parser").GetValue(null, null);
        temp = temp.ParseFrom(data);
        foreach (var item in temp.Data)
        {
            tempRow.Clear();
            foreach (var i in serializer.fieldsInfo)
                tempRow.Add(i.GetValue(item, null));

            tempRows.Add(tempRow.ToArray());
        }

        tempFields.Clear();
        foreach (var item in temp.Fields)
            tempFields.Add(item);

        return new DataTable(tempFields, tempRows);
    }
}

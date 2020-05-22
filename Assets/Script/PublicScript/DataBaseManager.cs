﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

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
                serializers.Add(classname, new Serializer(assembly.GetType(PROTOBUF_DLL + "." + classname), item));
            }
        }
    }

    public DataTable GetTableNormal(string tableName)
    {
        return GetTable(BundleManager.NormalD + tableName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tablePath">不包含normal</param>
    /// <returns></returns>
    public DataTable GetTable(string tablePath)
    {
        if (tables.TryGetValue(tablePath, out DataTable value))
            return value;

        if (unDeserializedTables.TryGetValue(tablePath, out byte[] bytes))
        {
            DataTable result = LoadFromProtobuf(serializers[tablePath], bytes);
            unDeserializedTables.Remove(tablePath);
            tables.Add(tablePath, result);
            return result;
        }

        int index = tablePath.LastIndexOf(Paths.Div);

        string bundlePath;
        if (index == -1)
            bundlePath = null;
        else
            bundlePath = tablePath.Substring(0, index);

        foreach (var item in LoadBundle(bundlePath))
        {
            if (!unDeserializedTables.TryGetValue(item.Item1, out _))
                unDeserializedTables.Add(item.Item1, item.Item2);
        }

        if (!unDeserializedTables.TryGetValue(tablePath, out _))
        {
            Debug.LogError("No such table named " + tablePath);
            throw new Exception();
        }

        return GetTable(tablePath);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bundlePath">不包含normal</param>
    /// <returns></returns>
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
        //dynamic temp = serializer.serializer.GetProperty("Parser").GetValue(null, null);
        //temp = temp.ParseFrom(data);
        //foreach (var item in temp.Data)
        //{
        //    tempRow.Clear();
        //    foreach (var i in serializer.fieldsInfo)
        //        tempRow.Add(i.GetValue(item, null));

        //    tempRows.Add(tempRow.ToArray());
        //}

        object parser = serializer.serializer.GetProperty("Parser").GetValue(null, null);
        object temp = parser.GetType().GetMethod("ParseFrom", new Type[] { typeof(byte[]) }).Invoke(parser, new object[] { data });

        //Type repeatedFieldType = typeof(Google.Protobuf.Collections.RepeatedField<>);
        parser = serializer.serializer.GetProperty("Data").GetValue(temp, null);
        System.Collections.IEnumerator enumerator = (System.Collections.IEnumerator)parser.GetType().GetMethod("GetEnumerator").Invoke(parser, null);

        while (enumerator.MoveNext())
        {
            object item = enumerator.Current;
            tempRow.Clear();
            foreach (var i in serializer.fieldsInfo)
                tempRow.Add(i.GetValue(item, null));

            tempRows.Add(tempRow.ToArray());
        }

        parser = serializer.serializer.GetProperty("Fields").GetValue(temp, null);
        enumerator = (System.Collections.IEnumerator)parser.GetType().GetMethod("GetEnumerator").Invoke(parser, null);

        tempFields.Clear();
        while (enumerator.MoveNext())
        {
            object item = enumerator.Current;
            tempFields.Add(item as string);
        }

        //tempFields.Clear();
        //foreach (var item in temp.Fields)
        //    tempFields.Add(item);

        return new DataTable(tempFields, tempRows);
    }
}

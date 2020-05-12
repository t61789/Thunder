using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using Google.Protobuf;
using Tool;

public class DataBaseManager
{
    private const string PROTOBUF_DLL = "ExcelDatabase";

    private const string ROWS = "rows";
    private const string FIELDS = "fields";
    private const string SAVE = "save\\";
    private readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();
    private readonly SortedList<int, string> fieldsIndex = new SortedList<int, string>();
    private readonly List<object[]> rows = new List<object[]>();
    private readonly List<object> row = new List<object>();
    private readonly string saveBasePath;
    private readonly List<string> fieldsBuff = new List<string>();
    private readonly Dictionary<string, Serializer> serializers = new Dictionary<string, Serializer>();

    private struct Serializer
    {
        public Type container;
        public Type serializer;

        public PropertyInfo[] fieldsInfo;

        public Serializer(Type container, Type serializer):this()
        {
            this.container = container;
            this.serializer = serializer;
        }
    }

    public DataTable this[string tableName]
    {
        get
        {
            if (tables.TryGetValue(tableName, out DataTable value))
                return value;
            else
                return new DataTable();
        }
    }

    public DataBaseManager()
    {
        try
        {
            saveBasePath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments)+Path.DirectorySeparatorChar+"Save";

            LoadAllSerializerDll();

            foreach (TextAsset item in PublicVar.bundleManager.GetAllAsset<TextAsset>("database"))
            {
                //tables.Add(item.name, LoadFromXmlDoc(XDocument.Parse(item.text)));
                tables.Add(item.name,LoadFromProtobuf(serializers[item.name],item.bytes));
            }
            PublicVar.bundleManager.ReleaseBundle("database");
        }
        catch (Exception e)
        {
            Debug.LogError("Load database failed");
            Debug.LogError(e);
        }
    }

    private void LoadAllSerializerDll()
    {
        Assembly assembly = Assembly.Load(PublicVar.bundleManager.GetAsset<TextAsset>(BundleManager.DllBundle,PROTOBUF_DLL).bytes);
        foreach (var item in assembly.GetTypes())
        {
            if (item.Name.Contains("Excel_"))
            {
                string classname = item.Name.Replace("Excel_", "");
                serializers.Add(classname,new Serializer(assembly.GetType(PROTOBUF_DLL+"."+classname),item));
            }
        }
    }

    public void LoadSave(string saveDir)
    {
        RemoveSaveTable();

        foreach (var item in Directory.GetFiles(saveBasePath +Path.DirectorySeparatorChar+saveDir + Path.DirectorySeparatorChar+"SaveTable", "*.xml"))
            tables.Add(SAVE + Path.GetFileNameWithoutExtension(item), LoadFromXmlDoc(XDocument.Load(item)));
    }

    public void RemoveSaveTable()
    {
        fieldsBuff.Clear();
        foreach (var item in tables.Keys)
        {
            if (item.Contains(SAVE))
                fieldsBuff.Add(SAVE);
        }
        foreach (var item in fieldsBuff)
            tables.Remove(item);
    }

    public object SelectOnce(string table, string field, (string, string)[] where = null)
    {
        return tables[table].Rows.Where(x =>
        {
            if (where != null)
                foreach (var i in where)
                    if (!x[i.Item1].Equals(i.Item2))
                        return false;
            return true;
        })?.First()[field];
    }

    public bool DeleteTable(string tableName)
    {
        return tables.Remove(tableName);
    }

    private DataTable LoadFromXmlDoc(XDocument doc)
    {
        fieldsBuff.Clear();

        foreach (var i in doc.Root.Element(FIELDS).Elements())
            fieldsBuff.Add(i.Name.ToString());

        rows.Clear();
        foreach (var i in doc.Root.Element(ROWS).Elements())
        {
            row.Clear();
            foreach (var j in i.Elements())
            {
                if (j.Value.Length == 0)
                    row.Add(null);
                else
                    row.Add(j.Value);
            }
            rows.Add(row.ToArray());
        }

        return new DataTable(fieldsBuff, rows);
    }

    private List<PropertyInfo> fieldsInfo = new List<PropertyInfo>();

    private DataTable LoadFromProtobuf(Serializer serializer,byte[] data)
    {
        if(serializer.fieldsInfo==null)
        {
            fieldsInfo.Clear();
            foreach (var item in serializer.container.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                fieldsInfo.Add(item);
            serializer.fieldsInfo = fieldsInfo.ToArray();
        }

        rows.Clear();
        dynamic temp = serializer.serializer.GetProperty("Parser").GetValue(null, null);
        temp = temp.ParseFrom(data);
        foreach (var item in temp.Data)
        {
            row.Clear();
            foreach (var i in serializer.fieldsInfo)
                row.Add(i.GetValue(item, null));
                
            rows.Add(row.ToArray());
        }

        fieldsBuff.Clear();
        foreach (var item in temp.Fields)
            fieldsBuff.Add(Tools.FirstCharLower(item));

        return new DataTable(fieldsBuff,rows);
    }
}

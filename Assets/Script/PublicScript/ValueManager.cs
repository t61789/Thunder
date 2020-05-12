using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class ValueManager
{
    private Dictionary<string, object> serialized = new Dictionary<string, object>();
    private Dictionary<string, XElement> unSerialized = new Dictionary<string, XElement>();

    public ValueManager()
    {
        try
        {
            foreach (var item in PublicVar.bundleManager.GetAllAsset<TextAsset>("values"))//文件
            {
                string sb = item.name;
                XDocument xdoc = XDocument.Parse(item.text);
                foreach (var item1 in xdoc.Root.Elements())//结构体
                    unSerialized.Add(sb + "/" + item1.Attribute("name").Value, item1);
            }
        }
        catch (Exception)
        {
            Debug.LogError("Load value settings failed");
            throw;
        }
    }

    public T GetValue<T>(string path)
    {
        if (serialized.TryGetValue(path, out object value))
        {
            try
            {
                return (T)value;
            }
            catch (Exception)
            {
                Debug.LogError(path + " is not type of " + typeof(T).Name);
                return default;
            }
        }
        else
        {
            if (unSerialized.TryGetValue(path, out XElement element))
            {
                try
                {
                    T result = (T)new XmlSerializer(typeof(T)).Deserialize(element.CreateReader());
                    unSerialized.Remove(path);
                    serialized.Add(path, result);
                    return result;
                }
                catch (Exception e)
                {
                    Debug.LogError(path + " Serialize failed");
                    Debug.LogError(e);
                    return default;
                }
            }
            else
            {
                Debug.LogError("No such struct named " + path);
                return default;
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;

public class ValueManager
{
    #region xml

    //private Dictionary<string, object> serialized = new Dictionary<string, object>();
    //private Dictionary<string, XElement> unSerialized = new Dictionary<string, XElement>();

    //public ValueManager()
    //{
    //    try
    //    {
    //        foreach (var item in PublicVar.bundleManager.GetAllAsset<TextAsset>("values"))//文件
    //        {
    //            string sb = item.name;
    //            XDocument xdoc = XDocument.Parse(item.text);
    //            foreach (var item1 in xdoc.Root.Elements())//结构体
    //                unSerialized.Add(sb + "/" + item1.Attribute("name").Value, item1);
    //        }
    //    }
    //    catch (Exception)
    //    {
    //        Debug.LogError("Load value settings failed");
    //        throw;
    //    }
    //}

    //public T GetValue<T>(string path)
    //{
    //    if (serialized.TryGetValue(path, out object value))
    //    {
    //        try
    //        {
    //            return (T)value;
    //        }
    //        catch (Exception)
    //        {
    //            Debug.LogError(path + " is not type of " + typeof(T).Name);
    //            return default;
    //        }
    //    }
    //    else
    //    {
    //        if (unSerialized.TryGetValue(path, out XElement element))
    //        {
    //            try
    //            {
    //                T result = (T)new XmlSerializer(typeof(T)).Deserialize(element.CreateReader());
    //                unSerialized.Remove(path);
    //                serialized.Add(path, result);
    //                return result;
    //            }
    //            catch (Exception e)
    //            {
    //                Debug.LogError(path + " Serialize failed");
    //                Debug.LogError(e);
    //                return default;
    //            }
    //        }
    //        else
    //        {
    //            Debug.LogError("No such struct named " + path);
    //            return default;
    //        }
    //    }
    //}

    #endregion

    private Dictionary<string, string> unDeserializedBuffer = new Dictionary<string, string>();
    private Dictionary<string, object> buffer = new Dictionary<string, object>();

    public T LoadValue<T>(string bundlePath,string valueName)
    {
        return LoadValue<T>(bundlePath+BundleManager.PathDivider+valueName);
    }

    public T LoadValue<T>(string valuePath)
    {
        if(buffer.TryGetValue(valuePath,out object target))
            return (T)target;

        if(unDeserializedBuffer.TryGetValue(valuePath,out string json))
        {
            T result = JsonConvert.DeserializeObject<T>(json);
            unDeserializedBuffer.Remove(valuePath);
            buffer.Add(valuePath,result);
            return result;
        }

        int index = valuePath.LastIndexOf(BundleManager.PathDivider);

        foreach (var item in LoadBundle(valuePath.Substring(0, index)))
        {
            if(!unDeserializedBuffer.TryGetValue(item.Item1, out _))
                unDeserializedBuffer.Add(item.Item1, item.Item2);
        }

        if (!unDeserializedBuffer.TryGetValue(valuePath, out _))
        {
            Debug.LogError("No such value named " + valuePath);
            throw new Exception();
        }

        return LoadValue<T>(valuePath);
    }

    private List<(string,string)> LoadBundle(string bundlePath)
    {
        List<(string, string)> result = new List<(string, string)>();

        StringBuilder pathBuilder = new StringBuilder();
        foreach (var item in PublicVar.bundleManager.GetAllAsset<TextAsset>(bundlePath))
        {
            pathBuilder.Clear();
            pathBuilder.Append(BundleManager.ValuesBundleD);
            pathBuilder.Append(bundlePath);
            pathBuilder.Append(BundleManager.PathDivider);
            pathBuilder.Append(item.name);
            result.Add((pathBuilder.ToString(), item.text));
        }

        PublicVar.bundleManager.ReleaseBundle(bundlePath);

        return result;
    }
}

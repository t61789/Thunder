using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

    private readonly Dictionary<string, string> unDeserializedBuffer = new Dictionary<string, string>();
    private readonly Dictionary<string, object> buffer = new Dictionary<string, object>();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="valuePath">不包含normal</param>
    /// <returns></returns>
    public T LoadValue<T>(string valuePath)
    {
        string bundlePath;

        int index = valuePath.LastIndexOf(Paths.Div);
        if (index == -1)
        {
            bundlePath = BundleManager.Normal;
            valuePath = bundlePath + Paths.Div + valuePath;
        }
        else
            bundlePath = valuePath.Substring(0, index);

        return LoadValueBase<T>(bundlePath, valuePath);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="valuePath">包含normal</param>
    /// <returns></returns>
    public T LoadValue<T>(string bundlePath, string valueName)
    {
        return LoadValueBase<T>(bundlePath, bundlePath + Paths.Div + valueName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bundlePath">包含normal</param>
    /// <param name="valuePath"></param>
    /// <returns></returns>
    private T LoadValueBase<T>(string bundlePath, string valuePath)
    {
        if (buffer.TryGetValue(valuePath, out object target))
            return (T)target;

        if (unDeserializedBuffer.TryGetValue(valuePath, out string json))
        {
            T result = JsonConvert.DeserializeObject<T>(json);
            unDeserializedBuffer.Remove(valuePath);
            buffer.Add(valuePath, result);
            return result;
        }

        foreach (var item in LoadBundle(bundlePath))
        {
            if (!unDeserializedBuffer.TryGetValue(item.Item1, out _))
                unDeserializedBuffer.Add(item.Item1, item.Item2);
        }

        if (!unDeserializedBuffer.TryGetValue(valuePath, out _))
        {
            Debug.LogError("No such value named " + valuePath);
            throw new Exception();
        }

        return LoadValueBase<T>(bundlePath, valuePath);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bundlePath">包含normal，不包含基础文件夹</param>
    /// <returns>包含normal，不包含基础文件夹</returns>
    private List<(string, string)> LoadBundle(string bundlePath)
    {
        List<(string, string)> result = new List<(string, string)>();

        StringBuilder pathBuilder = new StringBuilder();
        string temp = BundleManager.ValuesBundleD + bundlePath;
        foreach (var item in PublicVar.bundle.GetAllAsset<TextAsset>(temp))
        {
            pathBuilder.Clear();
            pathBuilder.Append(bundlePath);
            pathBuilder.Append(Paths.Div);
            pathBuilder.Append(item.name);
            result.Add((pathBuilder.ToString(), item.text));
        }

        PublicVar.bundle.ReleaseBundle(null,temp);

        return result;
    }
}

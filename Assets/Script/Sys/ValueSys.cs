using System.Collections.Generic;
using Newtonsoft.Json;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Sys
{
    public class ValueSys : IBaseSys
    {
        private static readonly string DefaultBundle = Paths.ValuesBundleD + Paths.Normal;

        private readonly Dictionary<AssetId, ValueUnit> _Values = new Dictionary<AssetId, ValueUnit>();

        public ValueSys()
        {
            Ins = this;
        }

        //private readonly Dictionary<string, string> unDeserializedBuffer = new Dictionary<string, string>();
        //private readonly Dictionary<string, object> buffer = new Dictionary<string, object>();

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="valuePath">不包含normal</param>
        ///// <returns></returns>
        //public T LoadValue<T>(string valuePath)
        //{
        //    string bundlePath;

        //    int index = valuePath.LastIndexOf(Paths.Div);
        //    if (index == -1)
        //    {
        //        bundlePath = Paths.Normal;
        //        valuePath = bundlePath + Paths.Div + valuePath;
        //    }
        //    else
        //        bundlePath = valuePath.Substring(0, index);

        //    return LoadValueBase<T>(bundlePath, valuePath);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="valuePath">包含normal</param>
        ///// <returns></returns>
        //public T LoadValue<T>(string bundlePath, string valueName)
        //{
        //    return LoadValueBase<T>(bundlePath, bundlePath + Paths.Div + valueName);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="bundlePath">包含normal</param>
        ///// <param name="valuePath"></param>
        ///// <returns></returns>
        //private T LoadValueBase<T>(string bundlePath, string valuePath)
        //{
        //    if (buffer.TryGetValue(valuePath, out object target))
        //        return (T)target;

        //    if (unDeserializedBuffer.TryGetValue(valuePath, out string json))
        //    {
        //        T result = JsonConvert.DeserializeObject<T>(json);
        //        unDeserializedBuffer.Dequeue(valuePath);
        //        buffer.Add(valuePath, result);
        //        return result;
        //    }

        //    foreach (var item in LoadBundle(bundlePath))
        //    {
        //        if (!unDeserializedBuffer.TryGetValue(item.Item1, out _))
        //            unDeserializedBuffer.Add(item.Item1, item.Item2);
        //    }

        //    if (!unDeserializedBuffer.TryGetValue(valuePath, out _))
        //    {
        //        Debug.LogError("No such value named " + valuePath);
        //        throw new Exception();
        //    }

        //    return LoadValueBase<T>(bundlePath, valuePath);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="bundlePath">包含normal，不包含基础文件夹</param>
        ///// <returns>包含normal，不包含基础文件夹</returns>
        //private List<(string, string)> LoadBundle(string bundlePath)
        //{
        //    List<(string, string)> result = new List<(string, string)>();

        //    StringBuilder pathBuilder = new StringBuilder();
        //    string temp = Paths.ValuesBundleD + bundlePath;
        //    foreach (var item in System.System.bundle.GetAllAsset<TextAsset>(temp))
        //    {
        //        pathBuilder.Clear();
        //        pathBuilder.Append(bundlePath);
        //        pathBuilder.Append(Paths.Div);
        //        pathBuilder.Append(item.name);
        //        result.Add((pathBuilder.ToString(), item.text));
        //    }

        //    System.System.bundle.ReleaseBundle(null,temp);

        //    return result;
        //}

        public static ValueSys Ins { get; private set; }

        public void OnSceneEnter(string preScene, string curScene)
        {
        }

        public void OnSceneExit(string curScene)
        {
        }

        public void OnApplicationExit()
        {
        }

        public T GetValue<T>(string valuePath)
        {
            return GetValue<T>(AssetId.Create(valuePath, DefaultBundle));
        }

        public T GetValue<T>(string bundleGroup, string bundle, string name)
        {
            return GetValue<T>(new AssetId(bundleGroup, bundle, name, DefaultBundle));
        }

        private T GetValue<T>(AssetId id)
        {
            for (var i = 0; i < 2; i++)
            {
                if (_Values.TryGetValue(id, out var value) && value.UnDes == null) return (T) value.Des;

                if (value.UnDes != null)
                {
                    var result = JsonConvert.DeserializeObject<T>(value.UnDes);
                    value.Des = result;
                    value.UnDes = null;
                    _Values[id] = value;
                    return result;
                }

                LoadBundle(id);

                Assert.IsTrue(_Values.TryGetValue(id, out _),
                    $"未在 {id.BundleGroup}!{id.Bundle} 内找到名为 {id.Name} 的table");
            }

            return default;
        }

        private void LoadBundle(AssetId id)
        {
            foreach (var asset in BundleSys.Ins.GetAllAsset<TextAsset>(id.BundleGroup, id.Bundle))
            {
                id.Name = asset.name;
                if (_Values.ContainsKey(id)) continue;
                _Values.Add(id, new ValueUnit(asset.text));
            }
        }

        private struct ValueUnit
        {
            public string UnDes;
            public object Des;

            public ValueUnit(string unDes)
            {
                UnDes = unDes;
                Des = null;
            }
        }

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
        //                unSerialized.Dequeue(path);
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
    }
}
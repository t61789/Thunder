using System.Collections.Generic;
using Newtonsoft.Json;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Tool
{
    public class ValueSys : IBaseSys
    {
        public static ValueSys Ins { get; private set; }
        private static readonly string DefaultBundle = Paths.ValuesBundleD + Paths.Normal;

        private readonly Dictionary<AssetId, ValueUnit> _Values = new Dictionary<AssetId, ValueUnit>();

        public ValueSys()
        {
            Ins = this;
        }

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
            return GetValue<T>(AssetId.Parse(valuePath, DefaultBundle));
        }

        public T GetValue<T>(string bundle, string name)
        {
            return GetValue<T>(new AssetId(bundle??DefaultBundle, name));
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
                    $"未在 {id.Bundle} 内找到名为 {id.Name} 的table");
            }

            return default;
        }

        private void LoadBundle(AssetId id)
        {
            foreach (var asset in BundleSys.Ins.GetAllAsset<TextAsset>( id.Bundle))
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
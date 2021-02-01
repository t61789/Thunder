using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using Newtonsoft.Json;
using UnityEngine;

namespace Thunder
{
    public class CtrlKeys
    {
        private static Dictionary<string, CtrlKey> _Keys;
        private static HashSet<string> _DeExitKeys;

        public static void Init()
        {
            if (_Keys != null) return;
            _Keys = LoadKeys(Config.CtrlKeysValuePath);
            _DeExitKeys = new HashSet<string>();
        }

        public static CtrlKey GetKey(string keyName)
        {
            if (_Keys.TryGetValue(keyName, out var value)) return value;

            if (_DeExitKeys.Contains(keyName))
            {
                Debug.Log($"指定操作键未定义 ${keyName}");
                _DeExitKeys.Add(keyName);
            }

            return default;
        }

        private static Dictionary<string, CtrlKey> LoadKeys(string keysPath)
        {
            var sel = JsonConvert.DeserializeObject<IEnumerable<Receiver>>(ValueSys.GetRawValue(keysPath));
            return sel.ToDictionary(
                key => key.Name, 
                key => new CtrlKey(key.KeyName, key.ShieldValue));
        }

        private struct Receiver
        {
            public string Name;
            public string KeyName;
            public float ShieldValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Framework
{
    public class TextSys : IBaseSys
    {
        private static Dictionary<string, string> _Dic 
            = new Dictionary<string, string>();

        public TextSys()
        {
            _Dic = (
                    from row in DataBaseSys.GetTable(Config.TextTableName)
                    select new { key = (string)row["key"], text = (string)row["text"] })
                .ToDictionary(x => x.key, x => x.text);
        }

        public static string GetText(string key)
        {
            return _Dic.TryGetAndLog(key,$"未找到指定文本 \"{key}\"");
        }

        public void OnSceneEnter(string preScene, string curScene) { }

        public void OnSceneExit(string curScene) { }

        public void OnApplicationExit() { }
    }
}
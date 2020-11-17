using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Framework
{
    public partial class TextSys : IBaseSys
    {
        public TextSys()
        {

            var dic = (
                    from row in DataBaseSys.GetTable("text")
                    select new { key = (string)row["key"], text = (string)row["text"] })
                .ToDictionary(x => x.key, x => x.text);
            SetStrValues(dic);
        }

        private static void SetStrValues(IReadOnlyDictionary<string, string> dic)
        {
            foreach (var field in typeof(TextSys).GetFields(BindingFlags.Static | BindingFlags.Public))
                field.SetValue(null, dic[field.Name]);
        }

        public void OnSceneEnter(string preScene, string curScene) { }

        public void OnSceneExit(string curScene) { }

        public void OnApplicationExit() { }
    }
}
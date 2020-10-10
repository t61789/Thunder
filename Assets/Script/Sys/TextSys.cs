using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Thunder.Sys
{
    public class TextSys : IBaseSys
    {
        public static string RequestGameSuccess;
        public static string CancelGameRequest;

        public TextSys(DataBaseSys dataBase)
        {
            var dic = (
                    from row in dataBase["text"]
                    select new {key = (string) row["key"], text = (string) row["text"]})
                .ToDictionary(x => x.key, x => x.text);
            SetStrValues(dic);
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

        private void SetStrValues(IReadOnlyDictionary<string, string> dic)
        {
            foreach (var field in GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
                field.SetValue(null, dic[field.Name]);
        }
    }
}
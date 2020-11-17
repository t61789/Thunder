using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Framework
{
    public class SaveSys
    {
        public static string SaveName;

        public static bool CreateSaveDir(string saveName)
        {
            var savePath = Paths.DocumentPath.PCombine(saveName);
            if (Directory.Exists(savePath))
                return false;

            var dirs = (
                from row in DataBaseSys.GetTable("directory_struct")
                where row["id"] == "save"
                select row).ToArray();

            var stack = new Stack<string>();
            stack.Push("");

            var result = new List<string>();

            const string name = "name";
            const string extension = "extension";
            const string parent = "parent";

            while (stack.Count != 0)
            {
                var node = stack.Pop();

                foreach (var item in dirs.Where(x => x[parent] == Path.GetFileName(node)))
                    stack.Push(node + Path.DirectorySeparatorChar + item[name] + item[extension]);
                result.Add(node);
            }


            foreach (var item in result)
                if (Path.GetExtension(item) == "")
                    Directory.CreateDirectory(savePath + item);
                else
                    File.Create(savePath + item).Close();

            return true;
        }

        public static void Save()
        {
            File.WriteAllText(
                Paths.DocumentPath.PCombine(SaveName).PCombine("data").PCombine("save.json"),
                JsonConvert.SerializeObject(SaveData.Ins));
        }

        public static SaveData LoadSave()
        {
            var saveJsonRPath = Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "save.json";
            var path = Paths.DocumentPath.PCombine(SaveName).PCombine(saveJsonRPath);

            SaveData result;
            try
            {
                var json = File.ReadAllText(path);
                result = JsonConvert.DeserializeObject<SaveData>(json);
            }
            catch (Exception)
            {
                Debug.LogError($"存档 {saveJsonRPath} 加载失败");
                throw;
            }

            SaveData.Ins = result;

            return result;
        }
    }
}
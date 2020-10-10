using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Thunder.Utility;

namespace Thunder.Sys
{
    [JsonObject(MemberSerialization.OptOut)]
    public class SaveSys
    {
        [JsonIgnore] public string curSaveName;

        public SortedSet<int> levelComplete;

        [JsonIgnore] private string savedJson;

        private SaveSys()
        {
            levelComplete = new SortedSet<int>();
        }

        //public Ship.CreateShipParam playerShipParam;

        public void Init(string curSaveName, string savedJson)
        {
            this.curSaveName = curSaveName;
            this.savedJson = savedJson;
        }

        public static SaveSys LoadSave(string saveName)
        {
            var saveJsonRPath = Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "save.json";

            var path = Paths.DocumentPathD + saveName + saveJsonRPath;

            var json = File.ReadAllText(path);

            var result = JsonConvert.DeserializeObject<SaveSys>(json);

            if (result == null) result = new SaveSys();
            result.Init(saveName, json);

            return result;
        }

        public static bool CreateSaveDir(string saveName)
        {
            var savePath = Paths.DocumentPathD + saveName;
            if (Directory.Exists(savePath))
                return false;

            //DataTable dirStruct = DataBaseSys.Ins["directory_struct"].Select(null, new (string, object)[] { ("id", "save") });
            var dirs = (
                from row
                    in DataBaseSys.Ins["directory_struct"]
                where row["id"] == "save"
                select row).ToArray();

            var stack = new Stack<string>();
            stack.Push("");

            var result = new List<string>();

            const string NAME = "name";
            const string EXTENSION = "extension";
            const string PARENT = "parent";

            while (stack.Count != 0)
            {
                var node = stack.Pop();

                foreach (var item in dirs.Where(x => x[PARENT] == Path.GetFileName(node)))
                    stack.Push(node + Path.DirectorySeparatorChar + item[NAME] + item[EXTENSION]);
                result.Add(node);
            }


            foreach (var item in result)
                if (Path.GetExtension(item) == "")
                    Directory.CreateDirectory(savePath + item);
                else
                    File.Create(savePath + item).Close();

            return true;
        }

        public void Save(string json = null)
        {
            if (json != null && !json.Equals(string.Empty))
                File.WriteAllText(
                    Paths.DocumentPathD + curSaveName + Path.DirectorySeparatorChar + "data" +
                    Path.DirectorySeparatorChar + "save.json", json);
            else
                File.WriteAllText(
                    Paths.DocumentPathD + curSaveName + Path.DirectorySeparatorChar + "data" +
                    Path.DirectorySeparatorChar + "save.json", JsonConvert.SerializeObject(this));
        }

        public string Check()
        {
            var unsavedJson = JsonConvert.SerializeObject(this);
            if (unsavedJson.GetHashCode() != savedJson.GetHashCode())
                return unsavedJson;
            return null;
        }
    }
}
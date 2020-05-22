using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

[JsonObject(MemberSerialization.OptOut)]
public class SaveManager
{
    [JsonIgnore]
    public string curSaveName;

    [JsonIgnore]
    private string savedJson;

    public SortedSet<int> levelComplete;

    public Ship.CreateShipParam playerShipParam;

    public void Init(string curSaveName, string savedJson)
    {
        this.curSaveName = curSaveName;
        this.savedJson = savedJson;
    }

    private SaveManager()
    {
        levelComplete = new SortedSet<int>();
    }

    public static SaveManager LoadSave(string saveName)
    {
        string saveJsonRPath = Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "save.json";

        string path = Paths.DocumentPathD + saveName + saveJsonRPath;

        string json = File.ReadAllText(path);

        SaveManager result = JsonConvert.DeserializeObject<SaveManager>(json);

        if (result == null) result = new SaveManager();
        result.Init(saveName, json);

        return result;
    }

    public static bool CreateSaveDir(string saveName)
    {
        string savePath = Paths.DocumentPathD + saveName;
        if (Directory.Exists(savePath))
            return false;

        DataTable dirStruct = PublicVar.dataBase["directory_struct"].Select(null, new (string, object)[] { ("id", "save") });

        Stack<string> stack = new Stack<string>();
        stack.Push("");

        List<string> result = new List<string>();

        const string NAME = "name";
        const string EXTENSION = "extension";
        const string PARENT = "parent";

        while (stack.Count != 0)
        {
            string node = stack.Pop();

            DataTable curTable = dirStruct.Select(null, new (string, object)[] { (PARENT, Path.GetFileName(node)) });
            foreach (var item in curTable)
                stack.Push(node + Path.DirectorySeparatorChar + item[NAME] as string + item[EXTENSION] as string);
            result.Add(node);
        }


        foreach (var item in result)
        {
            if (Path.GetExtension(item) == "")
                Directory.CreateDirectory(savePath + item);
            else
                File.Create(savePath + item).Close();
        }

        return true;
    }

    public void Save(string json = null)
    {
        if (json != null && !json.Equals(string.Empty))
            File.WriteAllText(Paths.DocumentPathD + curSaveName + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "save.json", json);
        else
            File.WriteAllText(Paths.DocumentPathD + curSaveName + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "save.json", JsonConvert.SerializeObject(this));
    }

    public string Check()
    {
        string unsavedJson = JsonConvert.SerializeObject(this);
        if (unsavedJson.GetHashCode() != savedJson.GetHashCode())
            return unsavedJson;
        else
            return null;
    }
}

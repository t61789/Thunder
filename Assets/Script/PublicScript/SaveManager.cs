using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityPlayerPrefs;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptOut)]
public class SaveManager
{
    [JsonIgnore]
    public string CurSaveName;
    [JsonIgnore]
    public static readonly string saveBasePath;

    public SortedSet<int> levelComplete;

    public Ship.CreateShipParam playerShipParam;

    static SaveManager()
    {
        saveBasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
            Path.DirectorySeparatorChar +
            "MyGames" +
            Path.DirectorySeparatorChar +
            "Thunder" +
            Path.DirectorySeparatorChar;
        if (!Directory.Exists(saveBasePath))
            Directory.CreateDirectory(saveBasePath);
    }

    private SaveManager()
    {
        levelComplete = new SortedSet<int>();
    }

    public static SaveManager LoadSave(string saveName)
    {
        string saveJsonRPath = Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "save.json";

        string path = saveBasePath + saveName + saveJsonRPath;

        SaveManager result = JsonConvert.DeserializeObject<SaveManager>(File.ReadAllText(path));

        if (result == null) result = new SaveManager();
        result.CurSaveName = saveName;

        return result;
    }

    public static bool CreateSaveDir(string saveName)
    {
        string savePath = saveBasePath + Path.DirectorySeparatorChar + saveName;
        if (Directory.Exists(savePath))
            return false;

        DataTable dirStruct = PublicVar.dataBase["directory_struct"].Select(null,new (string, object)[] {("id","save") });

        Stack<string> stack = new Stack<string>();
        stack.Push("");

        List<string> result = new List<string>();

        const string NAME = "name";
        const string EXTENSION = "extension";
        const string PARENT = "parent";

        while(stack.Count!=0)
        {
            string node = stack.Pop();

            DataTable curTable = dirStruct.Select(null,new (string, object)[] {(PARENT, Path.GetFileName(node)) });
            foreach (var item in curTable)
                stack.Push(node + Path.DirectorySeparatorChar + item[NAME] as string + item[EXTENSION] as string);
            result.Add(node);
        }

        
        foreach (var item in result)
        {
            if (Path.GetExtension(item) == "")
                Directory.CreateDirectory(savePath+item);
            else
                File.Create(savePath + item).Close();
        }

        return true;
    }

    public void Save()
    {
        File.WriteAllText(saveBasePath + CurSaveName + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "save.json", JsonConvert.SerializeObject(this));
    }
}

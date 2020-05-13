using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LevelPlane:ListPlane
{
    protected override void Awake()
    {
        base.Awake();

        int completeLevelIndex = 3;
        DataTable levelData = PublicVar.dataBaseManager["level"].Select();

        Load(completeLevelIndex,levelData);
    }

    private Dictionary<BaseButton, string> pairs = new Dictionary<BaseButton, string>(); 

    public void Load(int completeLevelIndex,DataTable levelData)
    {
        List<Action<BaseButton>> inits = new List<Action<BaseButton>>();
        List<string> diffNames = new List<string>();

        int count = 0;
        foreach (var item in levelData.Rows)
        {
            inits.Add(x=> {
                x.InitRect( UIInitAction.FillParent);

                Button but = x.GetComponent<Button>();

                Action temp = null;
                if (count > completeLevelIndex)
                    but.interactable = false;
                else
                    temp = () => StartLevel(x);

                x.Init(item["name"] as string,temp);

                count++;
            });

            diffNames.Add(item["diff_id"] as string);
        }

        BaseButton[] b= Init(new Parameters<BaseButton>(10, "normalButton", (100, 100), (10, 10), (0, 200), inits));
        for (int i = 0; i < b.Length; i++)
            pairs.Add(b[i],diffNames[i]);

        InitRect( UIInitAction.CenterParent);
    }

    public void StartLevel(BaseButton b)
    {
        Debug.Log(pairs[b]);

        //PublicVar.gameModeManager.SetupMode<SurvivalNoli>(x=>x.Init(null,pairs[b],20)).Start();

        pairs.Clear();
        PublicVar.uiManager.CloseUI(this);
    }
}

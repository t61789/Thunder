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

        DataTable levelData = PublicVar.dataBase["level"].Select();

        Load(PublicVar.saveManager.levelComplete.ToArray(), levelData);
    }

    private const string MODE_TYPE = "mode_type";
    private const string ARG = "arg";
    private const string NAME = "name";
    private readonly Dictionary<BaseButton, (string modeType,string arg)> pairs = new Dictionary<BaseButton, (string,string)>(); 

    public void Load(int[] completeLevels,DataTable levelData)
    {
        List<Action<BaseButton>> inits = new List<Action<BaseButton>>();
        List<(string,string)> arg = new List<(string, string)>();

        int count = 0;
        int index = 0;
        foreach (var item in levelData.Rows)
        {
            inits.Add(x=> {
                x.InitRect( UIInitAction.FillParent);

                Button but = x.GetComponent<Button>();

                Action temp = null;
                if (index<completeLevels.Length && completeLevels[index]==count)
                {
                    temp = () => StartLevel(x);
                    index++;
                }
                else
                    but.interactable = false;

                x.Init(item[NAME] as string, temp);

                count++;
            });

            arg.Add((item[MODE_TYPE] as string, item[ARG] as string));
        }

        BaseButton[] b= Init(new Parameters<BaseButton>(10, "normalButton", (0, 0), (5, 5), (0, 200), inits));
        for (int i = 0; i < b.Length; i++)
            pairs.Add(b[i],arg[i]);

        InitRect( UIInitAction.CenterParent);
    }

    public void StartLevel(BaseButton b)
    {
        var t = PublicVar.saveManager.playerShipParam;
        Ship player = PublicVar.player.SetPlayer(t);

        PublicVar.gameMode.SetupMode(pairs[b].modeType, x => x.Init(player.transform, pairs[b].arg)).Start();

        pairs.Clear();
        PublicVar.uiManager.CloseUI(this);
    }
}

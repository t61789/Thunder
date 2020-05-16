using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LevelPlane : ListPlane
{
    private const string MODE_TYPE = "mode_type";
    private const string ARG = "arg";
    private const string NAME = "name";
    private readonly Dictionary<BaseButton, (string modeType, string arg)> pairs = new Dictionary<BaseButton, (string, string)>();

    private int curLevelIndex = -1;
    private int maxLevel;

    private DataTable levelData;

    public bool unsaved;

    protected override void Awake()
    {
        base.Awake();
        AfterOpen();
    }

    public override void AfterOpen()
    {
        base.AfterOpen();

        pairs.Clear();

        levelData = PublicVar.dataBase["level"].Select();
        Clear();
        Load(PublicVar.saveManager.levelComplete.ToArray(), levelData);
    }

    public void Load(int[] completeLevels, DataTable levelData)
    {
        List<Action<BaseButton>> inits = new List<Action<BaseButton>>();
        List<(string, string)> arg = new List<(string, string)>();

        int count = 0;
        int index = 0;
        Debug.Log(index);
        foreach (var item in levelData.Rows)
        {
            DataTable.Row _item = item;

            bool flag = index < completeLevels.Length && completeLevels[index] == count;
            if (flag)
            {
                //Debug.Log(index);
                index++;
            }

            inits.Add(x =>
            {
                x.InitRect(UIInitAction.FillParent);
                Button but = x.GetComponent<Button>();
                Action temp = null;
                if (flag)
                {
                    temp = () => StartLevel(x);
                    but.interactable = true;
                }
                else
                    but.interactable = false;
                x.Init(_item[NAME] as string, temp);
            });
            count++;
            arg.Add((item[MODE_TYPE] as string, item[ARG] as string));
        }
        maxLevel = count;

        BaseButton[] b = Init(new Parameters<BaseButton>(10, "normalButton", (0, 0), (5, 5), (0, 200), inits));
        for (int i = 0; i < b.Length; i++)
            pairs.Add(b[i], arg[i]);

        InitRect(UIInitAction.CenterParent);
    }

    public void StartLevel(BaseButton b)
    {
        var t = PublicVar.saveManager.playerShipParam;
        Ship player = PublicVar.player.SetPlayer(t);

        curLevelIndex = pairs.Keys.ToList().IndexOf(b);

        BaseGameMode bgm = PublicVar.gameMode.SetupMode(pairs[b].modeType, x => x.Init(player.transform, pairs[b].arg));
        bgm.OnModeComplete += GameModeComplete;
        bgm.Start();

        pairs.Clear();
        PublicVar.uiManager.CloseUI(this);
    }

    public void GameModeComplete(BaseGameMode b)
    {
        unsaved = true;

        if (curLevelIndex < maxLevel)
            PublicVar.saveManager.levelComplete.Add(curLevelIndex + 1);
        PublicVar.gameMode.RemoveMode();
        PublicVar.uiManager.OpenUI(UIName);
    }

    public void OpenMenu()
    {
        PublicVar.uiManager.OpenUI<BaseUI>("MenuPanel", this, true, UIInitAction.CenterParent, null);
    }

    public void Exit(BaseUI menuPanel)
    {
        if (unsaved)
        {
            ConfirmDialog confirmDialog = PublicVar.uiManager.OpenUI<ConfirmDialog>("confirmDialog", menuPanel, true, UIInitAction.CenterParent, x => x.Init("You have unsaved data, do you want to save them right now?"));
            confirmDialog.OnBeforeClose += DialogConfirmed;
            return;
        }

        PublicVar.instance.LoadSceneAsync("StartScene");
    }

    private void DialogConfirmed(BaseUI confirmDialog)
    {
        ConfirmDialog confirmDialogg = confirmDialog as ConfirmDialog;
        if (confirmDialogg.dialogResult == DialogResult.OK)
        {
            unsaved = false;
            Exit(null);
        }
        else
            SaveAndExit();
    }

    public void SaveAndExit()
    {
        Save();
        Exit(null);
    }

    public void Save()
    {
        PublicVar.saveManager.Save();
    }
}

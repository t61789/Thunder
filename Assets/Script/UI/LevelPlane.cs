using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelPlane : ListPlane
{
    public BaseUI menuPanel;
    private readonly Dictionary<BaseButton, int> pairs = new Dictionary<BaseButton, int>();
    private string unsavedJson;

    protected override void Awake()
    {
        base.Awake();
        Load();
    }

    private void Load()
    {
        List<Action<BaseButton>> inits = new List<Action<BaseButton>>();
        int[] completed = PublicVar.saveManager.levelComplete.ToArray();

        //int count = 0;
        //int index = 0;

        int completeCount = 0;
        for (int i = 0; i < PublicVar.level.levels.Length; i++)
        {
            var _item = PublicVar.level.levels[i];
            bool flag = _item.index == completed[completeCount];
            if (flag) completeCount = completeCount + 1 == completed.Length ? completeCount : completeCount + 1;
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
                x.Init(_item.name, temp);
            });
        }

        //foreach (var item in PublicVar.level.levels)
        //{
        //    var _item = item;
        //    bool flag = index < PublicVar.level.levels.Length && _item.index == count;
        //    if (flag)
        //        index++;

        //    inits.Add(x =>
        //    {
        //        x.InitRect(UIInitAction.FillParent);
        //        Button but = x.GetComponent<Button>();
        //        Action temp = null;
        //        if (flag)
        //        {
        //            temp = () => StartLevel(x);
        //            but.interactable = true;
        //        }
        //        else
        //            but.interactable = false;
        //        x.Init(_item.name, temp);
        //    });
        //    count++;
        //}
        BaseButton[] b = Init(new Parameters<BaseButton>(10, "normalButton", (0, 0), (5, 5), (0, 200), inits));
        for (int i = 0; i < b.Length; i++)
            pairs.Add(b[i], i);

        InitRect(UIInitAction.CenterParent);
    }

    public void StartLevel(BaseButton b)
    {
        GlobalBuffer.battleSceneParam = (PublicVar.level.levels[pairs[b]], null);
        PublicVar.instance.LoadSceneAsync("BattleScene");
    }

    public void OpenMenu()
    {
        PublicVar.uiManager.OpenUI<BaseUI>("MenuPanel", this, true, UIInitAction.CenterParent, null);
    }

    public void Exit(bool force)
    {
        if (!force)
        {
            unsavedJson = PublicVar.saveManager.Check();
            if (unsavedJson != null)
            {
                ConfirmDialog confirmDialog = PublicVar.uiManager.OpenUI<ConfirmDialog>("confirmDialog", menuPanel, true, UIInitAction.CenterParent, x => x.Init("You have unsaved data, do you want to save them right now?"));
                confirmDialog.OnBeforeClose += DialogConfirmed;
                return;
            }
        }

        PublicVar.instance.LoadSceneAsync("StartScene");
    }

    private void DialogConfirmed(BaseUI confirmDialog)
    {
        ConfirmDialog confirmDialogg = confirmDialog as ConfirmDialog;
        if (confirmDialogg.dialogResult == DialogResult.OK)
            Save(unsavedJson);

        Exit(true);
    }

    public void SaveAndExit()
    {
        Save();
        Exit(true);
    }

    public void Save(string json = null)
    {
        PublicVar.saveManager.Save(json);
    }
}

using System;
using System.Collections.Generic;



namespace Thunder.UI
{
    public class LevelPlane : ListPlane
    {
        //private readonly Dictionary<BaseButton, int> pairs = new Dictionary<BaseButton, int>();
        //public BaseUI menuPanel;
        //private string unsavedJson;

        //protected override void Awake()
        //{
        //    base.Awake();
        //    Load();
        //}

        //private void Load()
        //{
        //    var inits = new List<Action<BaseButton>>();
        //    int[] completed = null; //Sys.Stable.Save.levelComplete.ToArray();

        //    int count = 0;
        //    int index = 0;

        //    var completeCount = 0;
        //    for (int i = 0; i < Sys.Stable.Level.levels.Length; i++)
        //    {
        //        var _item = Sys.Stable.Level.levels[i];
        //        bool flag = _item.index == completed[completeCount];
        //        if (flag) completeCount = completeCount + 1 == completed.Length ? completeCount : completeCount + 1;
        //        inits.Add(x =>
        //        {
        //            x.InitRect(UiInitType.FillParent);
        //            Button but = x.GetComponent<Button>();
        //            Action temp = null;
        //            if (flag)
        //            {
        //                temp = () => StartLevel(x);
        //                but.interactable = true;
        //            }
        //            else
        //                but.interactable = false;
        //            x.Init(_item.name, temp);
        //        });
        //    }

        //    foreach (var item in PublicVar.level.levels)
        //    {
        //        var _item = item;
        //        bool flag = index < PublicVar.level.levels.Length && _item.index == count;
        //        if (flag)
        //            index++;

        //        inits.Add(x =>
        //        {
        //            x.InitRect(UIInitAction.FillParent);
        //            Button but = x.GetComponent<Button>();
        //            Action temp = null;
        //            if (flag)
        //            {
        //                temp = () => StartLevel(x);
        //                but.interactable = true;
        //            }
        //            else
        //                but.interactable = false;
        //            x.Init(_item.name, temp);
        //        });
        //        count++;
        //    }
        //    var b = Init(new Parameters(10, "normalButton", (0, 0), (5, 5), (0, 200)), inits);
        //    for (var i = 0; i < b.Length; i++)
        //        pairs.Add(b[i], i);

        //    InitRect(UiInitType.CenterParent);
        //}

        //public void StartLevel(BaseButton b)
        //{
        //    GlobalBuffer.battleSceneParam = (Sys.Stable.Level.levels[pairs[b]], null);
        //    Sys.Stable.Ins.LoadSceneAsync("BattleScene");
        //}

        //public void OpenMenu()
        //{
        //    UISys.Ins.OpenUI<BaseUI>("MenuPanel", UIName, true, UiInitType.CenterParent);
        //}

        //public void Exit(bool force)
        //{
        //    if (!force)
        //        unsavedJson = Sys.Stable.Save.Check();
        //        if (unsavedJson != null)
        //        {
        //            var confirmDialog = UISys.Ins.OpenUI<ConfirmDialog>("confirmDialog", menuPanel.UIName, true,
        //                UiInitType.CenterParent,
        //                x => x.Init("You have unsaved data, do you want to save them right now?"));
        //            confirmDialog.OnBeforeClose += DialogConfirmed;
        //            return;
        //        }

        //    Sys.Stable.Ins.LoadSceneAsync("StartScene");
        //}

        //private void DialogConfirmed(BaseUI confirmDialog)
        //{
        //    var confirmDialogg = confirmDialog as ConfirmDialog;
        //    if (confirmDialogg.dialogResult == DialogResult.Ok)
        //        Save(unsavedJson);

        //    Exit(true);
        //}

        //public void SaveAndExit()
        //{
        //    Save();
        //    Exit(true);
        //}

        //public void Save(string json = null)
        //{
        //    Sys.Stable.Save.Save(json);
        //}
    }
}
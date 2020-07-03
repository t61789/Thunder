using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.PublicScript;
using Assets.Script.Utility;
using UnityEngine.UI;

namespace Assets.Script.UI
{
    public class LevelPlane : ListPlane
    {
        public BaseUi menuPanel;
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
            int[] completed = System.System.saveManager.levelComplete.ToArray();

            //int count = 0;
            //int index = 0;

            int completeCount = 0;
            for (int i = 0; i < System.System.level.levels.Length; i++)
            {
                var _item = System.System.level.levels[i];
                bool flag = _item.index == completed[completeCount];
                if (flag) completeCount = completeCount + 1 == completed.Length ? completeCount : completeCount + 1;
                inits.Add(x =>
                {
                    x.InitRect(UiInitAction.FillParent);
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

            InitRect(UiInitAction.CenterParent);
        }

        public void StartLevel(BaseButton b)
        {
            GlobalBuffer.battleSceneParam = (System.System.level.levels[pairs[b]], null);
            System.System.instance.LoadSceneAsync("BattleScene");
        }

        public void OpenMenu()
        {
            System.System.UiSys.OpenUi<BaseUi>("MenuPanel", this, true, UiInitAction.CenterParent, null);
        }

        public void Exit(bool force)
        {
            if (!force)
            {
                unsavedJson = System.System.saveManager.Check();
                if (unsavedJson != null)
                {
                    ConfirmDialog confirmDialog = System.System.UiSys.OpenUi<ConfirmDialog>("confirmDialog", menuPanel, true, UiInitAction.CenterParent, x => x.Init("You have unsaved data, do you want to save them right now?"));
                    confirmDialog.OnBeforeClose += DialogConfirmed;
                    return;
                }
            }

            System.System.instance.LoadSceneAsync("StartScene");
        }

        private void DialogConfirmed(BaseUi confirmDialog)
        {
            ConfirmDialog confirmDialogg = confirmDialog as ConfirmDialog;
            if (confirmDialogg.dialogResult == DialogResult.Ok)
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
            System.System.saveManager.Save(json);
        }
    }
}

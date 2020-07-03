﻿using System;
using System.Collections.Generic;
using System.Linq;
using Thunder.Utility;
using UnityEngine.UI;

namespace Thunder.UI
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
            int[] completed = Sys.Stable.saveManager.levelComplete.ToArray();

            //int count = 0;
            //int index = 0;

            int completeCount = 0;
            for (int i = 0; i < Sys.Stable.level.levels.Length; i++)
            {
                var _item = Sys.Stable.level.levels[i];
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
            GlobalBuffer.battleSceneParam = (Sys.Stable.level.levels[pairs[b]], null);
            Sys.Stable.instance.LoadSceneAsync("BattleScene");
        }

        public void OpenMenu()
        {
            Sys.Stable.UiSys.OpenUi<BaseUi>("MenuPanel", this, true, UiInitAction.CenterParent, null);
        }

        public void Exit(bool force)
        {
            if (!force)
            {
                unsavedJson = Sys.Stable.saveManager.Check();
                if (unsavedJson != null)
                {
                    ConfirmDialog confirmDialog = Sys.Stable.UiSys.OpenUi<ConfirmDialog>("confirmDialog", menuPanel, true, UiInitAction.CenterParent, x => x.Init("You have unsaved data, do you want to save them right now?"));
                    confirmDialog.OnBeforeClose += DialogConfirmed;
                    return;
                }
            }

            Sys.Stable.instance.LoadSceneAsync("StartScene");
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
            Sys.Stable.saveManager.Save(json);
        }
    }
}

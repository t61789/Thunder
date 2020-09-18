using System;
using System.Collections.Generic;
using System.IO;
using Thunder.Sys;
using Thunder.Utility;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class LoadOrCreateSaveUi : BaseUI
    {
        public void StartLoadSave()
        {
            if (Sys.UISys.Ins.IsUIOpened("LoadSaveListPlane"))
            {
                Sys.UISys.Ins.CloseUI("LoadSaveListPlane");
                return;
            }

            List<Action<BaseUI>> inits = new List<Action<BaseUI>>();
            foreach (var item in Directory.GetDirectories(Paths.DocumentPath))
            {
                inits.Add(x =>
                {
                    Button b = x.GetComponent<Button>();
                    b.transform.Find("Text").GetComponent<Text>().text = Path.GetFileName(item);
                    b.onClick.AddListener(() => LoadSave(b));

                    x.InitRect(UiInitType.FillParent);
                });
            }

            Sys.UISys.Ins.OpenUI<ListPlane>("listPlane", UIName, true, UiInitType.CenterParent, x =>
                x.Init(new ListPlane.Parameters(1, "normalButton", (500, 150), (50, 50), (0, 0)), inits));
        }

        public void StartCreateSave()
        {
            Sys.Stable.Ins.LoadSceneAsync("CreateSaveScene");
        }

        public void LoadSave(Button button)
        {
            //Sys.Stable.Save = SaveSys.LoadSave(button.transform.Find("Text").GetComponent<Text>().text);
            Sys.Stable.Ins.LoadSceneAsync("LevelScene");
        }
    }


}

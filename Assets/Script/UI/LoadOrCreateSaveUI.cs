using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Thunder.Sys;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class LoadOrCreateSaveUi : BaseUi
    {
        public void StartLoadSave()
        {
            if (Sys.Stable.Ui.IsUiOpened("LoadSaveListPlane"))
            {
                Sys.Stable.Ui.CloseUi("LoadSaveListPlane");
                return;
            }

            List<Action<BaseUi>> inits = new List<Action<BaseUi>>();
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

            Sys.Stable.Ui.OpenUi<ListPlane>("listPlane", UiName, true, UiInitType.CenterParent, x =>
                x.Init(new ListPlane.Parameters(1, "normalButton", (500, 150), (50, 50), (0, 0)), inits));
        }

        public void StartCreateSave()
        {
            Sys.Stable.Instance.LoadSceneAsync("CreateSaveScene");
        }

        public void LoadSave(Button button)
        {
            Sys.Stable.Save = SaveSys.LoadSave(button.transform.Find("Text").GetComponent<Text>().text);
            Sys.Stable.Instance.LoadSceneAsync("LevelScene");
        }
    }

    
}

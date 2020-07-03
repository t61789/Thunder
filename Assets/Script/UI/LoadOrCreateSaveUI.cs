using System;
using System.Collections.Generic;
using System.IO;
using Thunder.Sys;
using Thunder.Utility;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class LoadOrCreateSaveUi : BaseUi
    {
        public void StartLoadSave()
        {
            if (Sys.Stable.UiSys.IsUiOpened("LoadSaveListPlane"))
            {
                Sys.Stable.UiSys.CloseUi("LoadSaveListPlane");
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

                    x.InitRect(UiInitAction.FillParent);
                });
            }

            Sys.Stable.UiSys.OpenUi<ListPlane>("listPlane", this, true, UiInitAction.CenterParent, x =>
                x.Init(new ListPlane.Parameters<BaseUi>(1, "normalButton", (500, 150), (50, 50), (0, 0), inits)));
        }

        public void StartCreateSave()
        {
            Sys.Stable.instance.LoadSceneAsync("CreateSaveScene");
        }

        public void LoadSave(Button button)
        {
            Sys.Stable.saveManager = SaveSys.LoadSave(button.transform.Find("Text").GetComponent<Text>().text);
            Sys.Stable.instance.LoadSceneAsync("LevelScene");
        }
    }
}

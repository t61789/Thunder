using System;
using System.Collections.Generic;
using System.IO;
using Assets.Script.PublicScript;
using Assets.Script.System;
using Assets.Script.Utility;
using UnityEngine.UI;

namespace Assets.Script.UI
{
    public class LoadOrCreateSaveUI : BaseUi
    {
        public void StartLoadSave()
        {
            if (System.System.UiSys.IsUiOpened("LoadSaveListPlane"))
            {
                System.System.UiSys.CloseUi("LoadSaveListPlane");
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

            System.System.UiSys.OpenUi<ListPlane>("listPlane", this, true, UiInitAction.CenterParent, x =>
            {
                x.Init(new ListPlane.Parameters<BaseUi>(1, "normalButton", (500, 150), (50, 50), (0, 0), inits));
            });
        }

        public void StartCreateSave()
        {
            System.System.instance.LoadSceneAsync("CreateSaveScene");
        }

        public void LoadSave(Button button)
        {
            System.System.saveManager = SaveSys.LoadSave(button.transform.Find("Text").GetComponent<Text>().text);
            System.System.instance.LoadSceneAsync("LevelScene");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

namespace Assets.Script.UI
{
    public class LoadOrCreateSaveUI : BaseUi
    {
        public void StartLoadSave()
        {
            if (PublicVar.uiManager.IsUiOpened("LoadSaveListPlane"))
            {
                PublicVar.uiManager.CloseUi("LoadSaveListPlane");
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

                    x.InitRect(UIInitAction.FillParent);
                });
            }

            PublicVar.uiManager.OpenUi<ListPlane>("listPlane", this, true, UIInitAction.CenterParent, x =>
            {
                x.Init(new ListPlane.Parameters<BaseUi>(1, "normalButton", (500, 150), (50, 50), (0, 0), inits));
            });
        }

        public void StartCreateSave()
        {
            PublicVar.instance.LoadSceneAsync("CreateSaveScene");
        }

        public void LoadSave(Button button)
        {
            PublicVar.saveManager = SaveManager.LoadSave(button.transform.Find("Text").GetComponent<Text>().text);
            PublicVar.instance.LoadSceneAsync("LevelScene");
        }
    }
}

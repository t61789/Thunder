using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class LoadOrCreateSaveUI : BaseUI
{
    public void StartLoadSave()
    {
        if (PublicVar.uiManager.IsUIOpened("LoadSaveListPlane"))
        {
            PublicVar.uiManager.CloseUI("LoadSaveListPlane");
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

                x.InitRect(UIInitAction.FillParent);
            });
        }

        PublicVar.uiManager.OpenUI<ListPlane>("listPlane", this, true, UIInitAction.CenterParent, x =>
        {
            x.Init(new ListPlane.Parameters<BaseUI>(1, "normalButton", (500, 150), (50, 50), (0, 0), inits));
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

using System;
using System.Collections.Generic;
using System.IO;
using Framework;
using Thunder.Utility;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class LoadOrCreateSaveUi : BaseUI
    {
        //public void StartLoadSave()
        //{
        //    if (UISys.Ins.IsUIOpened("LoadSaveListPlane"))
        //    {
        //        UISys.Ins.CloseUI("LoadSaveListPlane");
        //        return;
        //    }

        //    var inits = new List<Action<BaseUI>>();
        //    foreach (var item in Directory.GetDirectories(Paths.DocumentPath))
        //        inits.Add(x =>
        //        {
        //            var b = x.GetComponent<Button>();
        //            b.transform.Find("Text").GetComponent<Text>().text = Path.GetFileName(item);
        //            b.onClick.AddListener(() => LoadSave(b));

        //            x.InitRect(UiInitType.FillParent);
        //        });

        //    UISys.Ins.OpenUI<ListPlane>("listPlane", EntityName, true, UiInitType.CenterParent, x =>
        //        x.Init(new ListPlane.Parameters(1, "normalButton", (500, 150), (50, 50), (0, 0)), inits));
        //}

        //public void StartCreateSave()
        //{
        //    Sys.Stable.Ins.LoadSceneAsync("CreateSaveScene");
        //}

        //public void LoadSave(Button button)
        //{
        //    //Sys.Stable.Save = SaveSys.LoadSave(button.transform.Find("Text").GetComponent<Text>().text);
        //    Sys.Stable.Ins.LoadSceneAsync("LevelScene");
        //}
    }
}
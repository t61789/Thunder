using BehaviorDesigner.Runtime.Tasks.Unity.UnityPlayerPrefs;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadOrCreateSaveUI:BaseUI
{
    public void StartLoadSave()
    {
        if (PublicVar.uiManager.IsUIOpened("LoadSaveListPlane"))
        {
            PublicVar.uiManager.CloseUI("LoadSaveListPlane");
            return;
        }

        List<Action<BaseUI>> inits = new List<Action<BaseUI>>();
        foreach (var item in Directory.GetDirectories( SaveManager.saveBasePath))
        {
            inits.Add(x=> {
                Button b = x.GetComponent<Button>();
                b.transform.Find("Text").GetComponent<Text>().text = Path.GetFileName(item);
                b.onClick.AddListener(()=> LoadSave(b));

                x.InitRect( UIInitAction.FillParent);
            });
        }

        PublicVar.uiManager.OpenUI<ListPlane>("listPlane",this,true,UIInitAction.CenterParent,x=> {
            x.Init(new ListPlane.Parameters<BaseUI>(1, "normalButton", (200, 50), (10, 10),(0, 0),inits));
        });
    }

    public void StartCreateSave()
    {
        PublicVar.uiManager.OpenUI<InputDialog>("inputDialog",this,true, UIInitAction.CenterParent, x => {
            x.Init("");
            x.OnCloseCheck += (BaseUI baseUi, ref bool result) =>
            {
                if (!result) return;

                InputDialog id = baseUi as InputDialog;
                if (id.dialogResult == DialogResult.OK)
                {
                    result = SaveManager.CreateSaveDir(id.Text);
                    if (!result)
                        PublicVar.uiManager.OpenUI<MessageDialog>("messageDialog", x, true, UIInitAction.CenterParent,message=> {
                            message.Init("存档已存在");
                            message.rectTrans.anchoredPosition = Vector2.zero;
                        });
                    else
                    {
                        PublicVar.dataDeliver.Add("saveName", id.Text);
                        SceneManager.LoadScene(1);
                    }
                }
            };
        });
    }

    public void LoadSave(Button button)
    {
        PublicVar.dataDeliver.Add("saveName", button.transform.Find("Text").GetComponent<Text>().text);
        SceneManager.LoadScene(1);
    }
}

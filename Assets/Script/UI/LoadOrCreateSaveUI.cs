using BehaviorDesigner.Runtime.Tasks.Unity.UnityPlayerPrefs;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadOrCreateSaveUI:BaseUI
{
    [HideInInspector]
    public string saveBasePath;

    public override void Awake()
    {
        base.Awake();
        saveBasePath = PublicVar.gameDocument+Path.DirectorySeparatorChar +"Save";
        if (!Directory.Exists(saveBasePath))
            Directory.CreateDirectory(saveBasePath);
    }

    public void StartLoadSave()
    {
        if (PublicVar.uiManager.IsUIOpened("LoadSaveListPlane"))
        {
            PublicVar.uiManager.CloseUI("LoadSaveListPlane");
            return;
        }

        List<Action<BaseUI>> inits = new List<Action<BaseUI>>();
        foreach (var item in Directory.GetDirectories(saveBasePath))
        {
            inits.Add(x=> {
                Button b = x.GetComponent<Button>();
                b.transform.Find("Text").GetComponent<Text>().text = Path.GetFileName(item);
                b.onClick.AddListener(()=> LoadSave(b));

                x.rectTrans.anchorMax = Vector2.one;
                x.rectTrans.anchorMin = Vector2.zero;
                x.rectTrans.offsetMin = Vector2.zero;
                x.rectTrans.offsetMax = Vector2.zero;
                x.rectTrans.anchoredPosition = Vector2.zero;
            });
        }

        PublicVar.uiManager.OpenUI<ListPlane>("listPlane",this,true,OpenUIAction.MiddleOfParent,x=> {
            x.Init(new ListPlane.Parameters<BaseUI>(1, "normalButton", (200, 50), (10, 10),(0, 0),inits));
        });
    }

    public void StartCreateSave()
    {
        PublicVar.uiManager.OpenUI<InputDialog>("inputDialog",this,true, OpenUIAction.MiddleOfParent, x => {
            x.Init("");
            x.OnCloseCheck += (BaseUI baseUi, ref bool result) =>
            {
                if (!result) return;

                InputDialog id = baseUi as InputDialog;
                if (id.dialogResult == DialogResult.OK)
                {
                    result = SaveManager.CreateSaveDir(id.Text);
                    if (!result)
                        PublicVar.uiManager.OpenUI<MessageDialog>("messageDialog", x, true, OpenUIAction.MiddleOfParent,message=> {
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

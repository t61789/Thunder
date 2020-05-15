using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CreateSaveController:MonoBehaviour
{
    private void Awake()
    {
        PublicVar.uiManager.OpenUI<InputDialog>("inputDialog",  UIInitAction.CenterParent, x => {
            x.Init("");
            x.OnCloseCheck += (BaseUI baseUi, ref bool result) =>
            {
                if (!result) return;

                InputDialog id = baseUi as InputDialog;
                if (id.dialogResult == DialogResult.OK)
                {
                    result = SaveManager.CreateSaveDir(id.Text);
                    if (!result)
                        PublicVar.uiManager.OpenUI<MessageDialog>("messageDialog", x, true, UIInitAction.CenterParent, message =>
                        {
                            message.Init("存档已存在");
                        });
                    else
                    {
                        PublicVar.saveManager = SaveManager.LoadSave(id.Text);
                        StartBuildShip();
                    }
                }
                else
                    GoBack();
            };
        });
    }

    private void StartBuildShip()
    {
        PublicVar.uiManager.OpenUI<BuildShipPanel>("buildShipPanel").OnBuildShipComplete+=BuildShipClosed;
    }

    private void BuildShipClosed(BuildShipPanel b)
    {
        PublicVar.saveManager.playerShipParam = b.buildResult;
        PublicVar.saveManager.Save();

        PublicVar.instance.LoadSceneAsync("MainScene");
    }

    public void GoBack()
    {
        PublicVar.instance.LoadSceneAsync("StartScene");
    }
}

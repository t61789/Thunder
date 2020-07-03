using Assets.Script.PublicScript;
using Assets.Script.System;
using Assets.Script.Utility;
using UnityEngine;

namespace Assets.Script.UI
{
    public class CreateSaveController : MonoBehaviour
    {
        private void Awake()
        {
            System.System.UiSys.OpenUi<InputDialog>("inputDialog", UiInitAction.CenterParent, x =>
            {
                x.Init("");
                x.OnCloseCheck += (BaseUi baseUi, ref bool result) =>
                {
                    if (!result) return;

                    InputDialog id = baseUi as InputDialog;
                    if (id.dialogResult == DialogResult.Ok)
                    {
                        result = SaveSys.CreateSaveDir(id.Text);
                        if (!result)
                            System.System.UiSys.OpenUi<MessageDialog>("messageDialog", x, true, UiInitAction.CenterParent, message =>
                            {
                                message.Init("存档已存在");
                            });
                        else
                        {
                            System.System.saveManager = SaveSys.LoadSave(id.Text);
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
            System.System.UiSys.OpenUi<BuildShipPanel>("buildShipPanel").OnBuildShipComplete += BuildShipClosed;
        }

        private void BuildShipClosed(BuildShipPanel b)
        {
            System.System.saveManager.playerShipParam = b.buildResult;
            System.System.saveManager.Save();

            System.System.instance.LoadSceneAsync("LevelScene");
        }

        public void GoBack()
        {
            System.System.instance.LoadSceneAsync("StartScene");
        }
    }
}

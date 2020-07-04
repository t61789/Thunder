using Thunder.Sys;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.UI
{
    public class CreateSaveController : MonoBehaviour
    {
        private void Awake()
        {
            Sys.Stable.Ui.OpenUi<InputDialog>("inputDialog", UiInitAction.CenterParent, x =>
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
                            Sys.Stable.Ui.OpenUi<MessageDialog>("messageDialog", x, true, UiInitAction.CenterParent, message =>
                            {
                                message.Init("存档已存在");
                            });
                        else
                        {
                            Sys.Stable.Save = SaveSys.LoadSave(id.Text);
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
            Sys.Stable.Ui.OpenUi<BuildShipPanel>("buildShipPanel").OnBuildShipComplete += BuildShipClosed;
        }

        private void BuildShipClosed(BuildShipPanel b)
        {
            Sys.Stable.Save.playerShipParam = b.buildResult;
            Sys.Stable.Save.Save();

            Sys.Stable.Instance.LoadSceneAsync("LevelScene");
        }

        public void GoBack()
        {
            Sys.Stable.Instance.LoadSceneAsync("StartScene");
        }
    }
}

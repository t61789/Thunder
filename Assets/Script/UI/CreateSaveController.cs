using Thunder.Sys;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.UI
{
    public class CreateSaveController : MonoBehaviour
    {
        private void Awake()
        {
            Sys.Stable.Ui.OpenUi("inputDialog", UiInitType.CenterParent, y =>
            {
                InputDialog x = y as InputDialog;
                x.Init("");
                x.OnCloseCheck += (BaseUi baseUi, ref bool result) =>
                {
                    if (!result) return;

                    InputDialog id = baseUi as InputDialog;
                    if (id.dialogResult == DialogResult.Ok)
                    {
                        result = SaveSys.CreateSaveDir(id.Text);
                        if (!result)
                            Sys.Stable.Ui.OpenUi("messageDialog", x.UiName, true, UiInitType.CenterParent, message =>
                           (message as MessageDialog).Init("存档已存在"));
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
            (Sys.Stable.Ui.OpenUi("buildShipPanel") as BuildShipPanel).OnBuildShipComplete += BuildShipClosed;
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

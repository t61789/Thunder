using UnityEngine;

namespace Assets.Script.UI
{
    public class CreateSaveController : MonoBehaviour
    {
        private void Awake()
        {
            PublicVar.uiManager.OpenUi<InputDialog>("inputDialog", UIInitAction.CenterParent, x =>
            {
                x.Init("");
                x.OnCloseCheck += (BaseUI baseUi, ref bool result) =>
                {
                    if (!result) return;

                    InputDialog id = baseUi as InputDialog;
                    if (id.dialogResult == DialogResult.OK)
                    {
                        result = SaveManager.CreateSaveDir(id.Text);
                        if (!result)
                            PublicVar.uiManager.OpenUi<MessageDialog>("messageDialog", x, true, UIInitAction.CenterParent, message =>
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
            PublicVar.uiManager.OpenUi<BuildShipPanel>("buildShipPanel").OnBuildShipComplete += BuildShipClosed;
        }

        private void BuildShipClosed(BuildShipPanel b)
        {
            PublicVar.saveManager.playerShipParam = b.buildResult;
            PublicVar.saveManager.Save();

            PublicVar.instance.LoadSceneAsync("LevelScene");
        }

        public void GoBack()
        {
            PublicVar.instance.LoadSceneAsync("StartScene");
        }
    }
}

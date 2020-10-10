using Thunder.Sys;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.UI
{
    public class CreateSaveController : MonoBehaviour
    {
        private void Awake()
        {
            UISys.Ins.OpenUI("inputDialog", UiInitType.CenterParent, y =>
            {
                var x = y as InputDialog;
                x.Init("");
                x.OnCloseCheck += (BaseUI baseUi, ref bool result) =>
                {
                    if (!result) return;

                    var id = baseUi as InputDialog;
                    if (id.dialogResult == DialogResult.Ok)
                    {
                        result = SaveSys.CreateSaveDir(id.Text);
                        if (!result)
                        {
                            UISys.Ins.OpenUI("messageDialog", x.UIName, true, UiInitType.CenterParent, message =>
                                (message as MessageDialog).Init("存档已存在"));
                        }
                    }
                    else
                    {
                        GoBack();
                    }
                };
            });
        }

        //private void StartBuildShip()
        //{
        //    (Sys.UISys.Ins.OpenUI("buildShipPanel") as BuildShipPanel).OnBuildShipComplete += BuildShipClosed;
        //}

        //private void BuildShipClosed(BuildShipPanel b)
        //{
        //    Sys.Stable.Save.playerShipParam = b.buildResult;
        //    Sys.Stable.Save.Save();

        //    Sys.Stable.Ins.LoadSceneAsync("LevelScene");
        //}

        public void GoBack()
        {
            Stable.Ins.LoadSceneAsync("StartScene");
        }
    }
}
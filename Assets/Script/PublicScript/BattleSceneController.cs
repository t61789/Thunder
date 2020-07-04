using Thunder.GameMode;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.PublicScript
{
    public class BattleSceneController : MonoBehaviour
    {
        public CheckoutPanel checkoutPanel;

        private LevelManager.LevelParam levelParam;

        private void Start()
        {
            LoadLevel();
        }

        public void LoadLevel()
        {
            var (levelParam, init) = GlobalBuffer.battleSceneParam;
            this.levelParam = levelParam;

            BaseGameMode gameMode = Sys.Stable.GameMode.SetupMode(levelParam.modeType, levelParam.arg, init);

            gameMode.OnModeComplete += GameModeComplete;
        }

        private void GameModeComplete(BaseGameMode gameMode, BaseGameMode.CompleteParam completeParam)
        {
            ControllerInput.Controlable = false;
            checkoutPanel.Init(completeParam);
            Sys.Stable.Ui.OpenUi(checkoutPanel.UiName);
            GlobalBuffer.battleSceneParam = (Sys.Stable.Level.LevelComplete(levelParam.index), null);
        }

        public void NextLevel()
        {
            ControllerInput.Controlable = true;
            Sys.Stable.Instance.LoadSceneAsync("BattleScene");
        }

        public void GoBack()
        {
            ControllerInput.Controlable = true;
            Sys.Stable.Instance.LoadSceneAsync("LevelScene");
        }
    }
}
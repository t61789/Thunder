using Assets.Script.GameMode;
using Assets.Script.UI;
using Assets.Script.Utility;
using UnityEngine;

namespace Assets.Script.PublicScript
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

            BaseGameMode gameMode = System.System.gameMode.SetupMode(levelParam.modeType, levelParam.arg, init);

            gameMode.OnModeComplete += GameModeComplete;
        }

        private void GameModeComplete(BaseGameMode gameMode, BaseGameMode.CompleteParam completeParam)
        {
            ControllerInput.Controlable = false;
            checkoutPanel.Init(completeParam);
            System.System.UiSys.OpenUi(checkoutPanel.UiName);
            GlobalBuffer.battleSceneParam = (System.System.level.LevelComplete(levelParam.index), null);
        }

        public void NextLevel()
        {
            ControllerInput.Controlable = true;
            System.System.instance.LoadSceneAsync("BattleScene");
        }

        public void GoBack()
        {
            ControllerInput.Controlable = true;
            System.System.instance.LoadSceneAsync("LevelScene");
        }
    }
}
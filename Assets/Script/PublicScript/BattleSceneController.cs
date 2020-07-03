using Assets.Script.GameMode;
using Assets.Script.UI;
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

            BaseGameMode gameMode = PublicVar.gameMode.SetupMode(levelParam.modeType, levelParam.arg, init);

            gameMode.OnModeComplete += GameModeComplete;
        }

        private void GameModeComplete(BaseGameMode gameMode, BaseGameMode.CompleteParam completeParam)
        {
            ControllerInput.Controlable = false;
            checkoutPanel.Init(completeParam);
            PublicVar.UiSys.OpenUi(checkoutPanel.UiName);
            GlobalBuffer.battleSceneParam = (PublicVar.level.LevelComplete(levelParam.index), null);
        }

        public void NextLevel()
        {
            ControllerInput.Controlable = true;
            PublicVar.instance.LoadSceneAsync("BattleScene");
        }

        public void GoBack()
        {
            ControllerInput.Controlable = true;
            PublicVar.instance.LoadSceneAsync("LevelScene");
        }
    }
}
using Thunder.Entity;
using Thunder.Sys;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Game
{
    public class FlyingSaucer:MonoBehaviour
    {
        public FlyingSaucerLauncher Launcher;
        public ScoreBoard UI;
        public AreaTrigger StandArea;
        public AreaTrigger GameArea;

        private void Start()
        {
            GameArea.Enter.AddListener(EnterGameArea);
            GameArea.Exit.AddListener(LeaveGameArea);
        }

        public void EnterGameArea(Collider c)
        {
            
        }

        public void LeaveGameArea(Collider c)
        {
            Stable.UI.CloseUI(UI.UIName);
        }

        public void RequestGame()
        {
            StandArea.Enter.AddListener(StartGame);
        }

        public void StartGame(Collider c)
        {
            Stable.UI.OpenUI(UI.UIName);
            UI.StartTurn();
        }

        public void EndGame()
        {

        }
    }
}

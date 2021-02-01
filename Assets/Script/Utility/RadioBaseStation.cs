using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using Thunder.UI;
using UnityEngine;

namespace Thunder
{
    public class RadioBaseStation:BaseBuilding,IInteractive
    {
        public int GameStartTimeCount = 10;
        public string GameStartCountWaitTextKey = "game_start_count_wait_title";
        public string GameStartCountProcessingTextKey = "game_start_count_processing_title";
        public string GameOverTextKey = "game_over";

        public static RadioBaseStation Ins { private set; get; }

        private bool _Counting;
        private bool _Started;
        private int _GameStartTimeCount;
        private Dropper _Dropper;
        private SimpleCounter _SecondCounter;

        protected override void Awake()
        {
            base.Awake();
            Ins = this;
            _Dropper = new Dropper(
                Config.DefaultDropForce, 
                () => Trans.position,
                () => Trans.rotation);
            _SecondCounter = new SimpleCounter(1);
        }

        private void Start()
        {
            GameStartAlertPanel.Ins.SetHeadTitle(TextSys.GetText(GameStartCountWaitTextKey));
        }

        private void FixedUpdate()
        {
            CheckCounting();
        }

        public void Interactive(ControlInfo info)
        {
            if (_Counting || _Started)
                return;
            if(info.Down) 
                StartTimeCount();
        }

        protected override void Dead()
        {
            base.Dead();

            FairyPanel.OpenPanel(GameStartAlertPanel.Ins.UiName);
            GameStartAlertPanel.Ins.SetHeadTitle(TextSys.GetText(GameOverTextKey));
            GameStartAlertPanel.Ins.SetTimeCount(null);
        }

        private void StartTimeCount()
        {
            _GameStartTimeCount = GameStartTimeCount;
            _Counting = true;
            _SecondCounter.Recount();
            GameStartAlertPanel.Ins.SetHeadTitle(TextSys.GetText(GameStartCountProcessingTextKey));
            GameStartAlertPanel.Ins.SetTimeCount(_GameStartTimeCount.ToString());
        }

        private void CheckCounting()
        {
            if (_Counting && _SecondCounter.Completed)
            {
                _SecondCounter.Recount();
                _GameStartTimeCount--;
                GameStartAlertPanel.Ins.SetTimeCount(_GameStartTimeCount.ToString());
                if (_GameStartTimeCount != 0) return;

                StartGame();
            }
        }

        private void StartGame()
        {
            _Started = true;
            _Counting = false;
            RespawnerCenter.Ins.Enable(true);
            FairyPanel.ClosePanel(GameStartAlertPanel.Ins.UiName);
        }
    }
}

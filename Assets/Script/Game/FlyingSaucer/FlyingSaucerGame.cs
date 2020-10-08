using System;
using System.Data.SqlTypes;
using System.Linq;
using Thunder.Entity;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Thunder.Game.FlyingSaucer
{
    public class FlyingSaucerGame:MonoBehaviour
    {
        public static FlyingSaucerGame Instance;

        public FlyingSaucerScoreBoard ScoreBoard;

        public float TurnTime;
        public float ScorePerHit;
        public float BatterFadeTime;
        public int MaxEffectiveBatter;
        public int MaxScoreMagnification;
        public delegate void DelDataChanged(float score,int batter, int hits);
        public static event DelDataChanged OnDataChanged;

        public AutoCounter TurnCounter { get; private set; }
        public AutoCounter BatterFadeCounter { get; private set; }

        private int _Batter;
        private float _CurScore;
        private int _CurHit;

        private void Awake()
        {
            PublicEvents.FlyingSaucerHit.AddListener(SaucerHit);


            BatterFadeCounter = new AutoCounter(this,BatterFadeTime).OnComplete(() =>
            {
                _Batter = 0;
                BroadCastData();
            });

            TurnCounter = new AutoCounter(this,TurnTime,false).OnComplete(() =>GameEnd(true));

            Instance = this;

            PublicEvents.GameStart.AddListener(GameStart);
            PublicEvents.GameStartDelay.AddListener(GameStartDelay);
        }

        private void Start()
        {
            string message = (
                from row
                    in DataBaseSys.Ins["message"]
                where row["Key"] == "flyingSaucerWelcome"
                select row["value"]).FirstOrDefault();
            LogPanel.Ins.LogSystem(message);
        }

        private void SaucerHit()
        {
            float magnification = Tools.Lerp(1, MaxScoreMagnification,
                Tools.InLerp(0, MaxEffectiveBatter, _Batter));
            _CurScore += ScorePerHit * magnification;
            _Batter++;
            _CurHit++;
            BatterFadeCounter.Recount();

            BroadCastData();
        }

        private void BroadCastData()
        {
            OnDataChanged?.Invoke(_CurScore,_Batter,_CurHit);
        }

        public void EnterGameArea(Collider c)
        {
            
        }

        public void LeaveGameArea(Collider c)
        {
            UISys.Ins.CloseUI(ScoreBoard.UIName);
        }

        private void GameStartDelay(GameType type)
        {
            if (type != GameType.FlyingSaucer) return;
            UISys.Ins.OpenUI(ScoreBoard.UIName);
        }

        private void GameStart(GameType type)
        {
            if (type != GameType.FlyingSaucer) return;
            _Batter = 0;
            _CurHit = 0;
            _CurScore = 0;
            TurnCounter.Resume();
        }

        private void GameEnd(bool completely)
        {
            PublicEvents.GameEnd?.Invoke( GameType.FlyingSaucer,true);
            string endMsg = $"Game over, you have got {_CurScore} score";
            LogPanel.Ins.LogSystem(endMsg);
            UISys.Ins.CloseUI(ScoreBoard.UIName);
            TurnCounter.Pause().Recount();
        }
    }
}

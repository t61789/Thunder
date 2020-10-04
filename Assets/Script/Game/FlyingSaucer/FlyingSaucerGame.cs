using System;
using System.Data.SqlTypes;
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

        public Counter TurnCounter { get; private set; }
        public Counter BatterFadeCounter { get; private set; }

        private int _Batter;
        private float _CurScore;
        private int _CurHit;

        private void Awake()
        {
            PublicEvents.FlyingSaucerHit.AddListener(SaucerHit);

            BatterFadeCounter = new Counter(BatterFadeTime).OnComplete(() =>
            {
                _Batter = 0;
                BroadCastData();
            }).ToAutoCounter(this,false);

            TurnCounter = new Counter(TurnTime).OnComplete(() =>GameEnd(true)).ToAutoCounter(this,false);

            Instance = this;

            PublicEvents.FlyingSaucerGameStart.AddListener(GameStart);
            PublicEvents.FlyingSaucerGameStartDelay.AddListener(GameStartDelay);
        }

        private void Start()
        {
            LogPanel.Instance.LogSystem((string)(DataBaseSys.Ins.GetTable("message").SelectOnce("value", new (string, object)[]{
                ("Key","flyingSaucerWelcome")})));
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

        private void GameStartDelay()
        {
            UISys.Ins.OpenUI(ScoreBoard.UIName);
        }

        private void GameStart()
        {
            _Batter = 0;
            _CurHit = 0;
            _CurScore = 0;
            TurnCounter.ResumeAutoCount(true);
        }

        private void GameEnd(bool completely)
        {
            PublicEvents.FlyingSaucerGameEnd?.Invoke(true);
            string endMsg = $"Game over, you have got {_CurScore} score";
            LogPanel.Instance.LogSystem(endMsg);
            UISys.Ins.CloseUI(ScoreBoard.UIName);
            TurnCounter.PauseAutoCount();
        }
    }
}

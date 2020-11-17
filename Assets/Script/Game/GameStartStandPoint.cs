using Framework;
using Thunder.Entity;
using Thunder.Utility;

using UnityEngine;

namespace Thunder.Game
{
    public class GameStartStandPoint : AreaTrigger
    {
        private int _Countdown;

        private AutoCounter _DelayCounter;
        private bool _Requested;
        private bool _Started;
        public float DelayTime;
        public GameType GameType;

        //todo 中途离开
        private void Awake()
        {
            PublicEvents.GameRequest.AddListener((x, y) =>
            {
                if (x != GameType)
                    return;
                _Requested = y;
            });
            PublicEvents.GameEnd.AddListener(GameEnd);
            _DelayCounter = new AutoCounter(this, DelayTime).OnComplete(GameStartDelayCompleted).Complete(false);
        }

        private void FixedUpdate()
        {
            if (_Countdown == 0 || !(_DelayCounter.TimeCount > DelayTime - _Countdown)) return;
            PublicEvents.LogMessage?.Invoke($"Game will start in {_Countdown} seconds");
            _Countdown--;
        }

        protected override void Stay(Collider collider)
        {
            if (!_Requested || _Started)
                return;

            _Started = true;

            PublicEvents.GameStartDelay?.Invoke(GameType);

            Player.Ins.FpsMover.Moveable = false;
        }

        private void GameEnd(GameType type, bool completely)
        {
            if (type != GameType)
                return;

            _Requested = false;
            _Started = false;

            Player.Ins.FpsMover.Moveable = true;
        }

        public void GameStartDelay(GameType type)
        {
            _DelayCounter.Recount();
            _Countdown = (int) DelayTime;
        }

        public void GameStartDelayCompleted()
        {
            PublicEvents.GameStart?.Invoke(GameType);
        }
    }
}
using System.Linq;
using Thunder.Tool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Game.SpotShooting
{
    public class SpotShootingGame : MonoBehaviour
    {
        private AutoCounter _NextTargetCounter;
        private SpotShootingTarget[] _Targets;

        private AutoCounter _TurnCounter;
        private int _UnableTargetNum;
        public float Difficulty = 1;
        public float DifficultyScoreFactor = 1.5f;
        public float RiseInterval = 1.5f;
        public float Score;
        public Transform TargetContainer;
        public float TurnTime = 20;

        private void Awake()
        {
            _Targets = (from Transform t
                    in TargetContainer
                select t.GetComponent<SpotShootingTarget>()).ToArray();
            PublicEvents.SpotShootingTargetHit.AddListener(TargetHit);
            PublicEvents.GameStart.AddListener(GameStart);
            _NextTargetCounter = new AutoCounter(this, RiseInterval, false).OnComplete(RiseNextTarget);
            _TurnCounter = new AutoCounter(this, TurnTime, false).OnComplete(GameEnd);
        }

        private void TargetHit(SpotShootingTarget target)
        {
            var preindex = _Targets.FindIndex(x => x == target);
            _Targets.Swap(preindex, _UnableTargetNum - 1);
            _UnableTargetNum--;
            Score += Difficulty * DifficultyScoreFactor;
        }

        private void RiseNextTarget()
        {
            if (_UnableTargetNum == _Targets.Length) return;
            var index = Tools.RandomInt(_UnableTargetNum, _Targets.Length - 1);
            _Targets[index].Rise();
            _Targets.Swap(_UnableTargetNum, index);
            _UnableTargetNum++;
            _NextTargetCounter.Recount();
        }

        private void GameStart(GameType type)
        {
            if (type != GameType.SpotShooting) return;
            _NextTargetCounter.Resume().Recount(RiseInterval / Difficulty);
            _TurnCounter.Resume();
        }

        private void GameEnd()
        {
            _TurnCounter.Pause().Recount();
            _NextTargetCounter.Pause().Recount();
            for (var i = 0; i < _UnableTargetNum; i++)
                _Targets[i].UnRise();
            _UnableTargetNum = 0;

            PublicEvents.LogMessage?.Invoke($"Game Over, you've got {(int) Score} score");

            Score = 0;
        }
    }
}
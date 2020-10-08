using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Entity;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Game
{
    public enum GameType
    {
        FlyingSaucer,
        SpotShooting
    }

    public class GameRequester:BaseEntity,IShootable
    {
        public GameType GameType;
        public float ReRequestTime = 3;
        public Color RequestedColor = Color.red;

        private bool _Started;
        private bool _Requested;
        private SimpleCounter _ReRequestSimpleCounter;
        private Color _BaseColor;

        private Material _Mat;

        protected override void Awake()
        {
            base.Awake();
            PublicEvents.GameStart.AddListener(x =>
            {
                if (x == GameType)
                    _Started = true;
            });
            PublicEvents.GameEnd.AddListener(GameEnd);
            _ReRequestSimpleCounter = new SimpleCounter(ReRequestTime, false);
            _Mat = GetComponent<MeshRenderer>().StandaloneMaterial();
            _BaseColor = _Mat.GetColor("_Light");
        }

        public void GetShoot(Vector3 hitPos, Vector3 hitDir, float damage)
        {
            if (!_ReRequestSimpleCounter.Completed || _Started) return;

            _Requested = !_Requested;
            _Mat.SetColor("_Light", _Requested ? RequestedColor : _BaseColor);
            PublicEvents.GameRequest?.Invoke(GameType,_Requested);
            _ReRequestSimpleCounter.Recount();
            LogPanel.Ins.LogSystem(_Requested
                ? "Please stand on the start point to start the game"
                : "You have canceled the game");
        }

        private void GameEnd(GameType type,bool completely)
        {
            if (type != GameType) return;
            _Requested = false;
            _Started = false;
            _Mat.SetColor("_Light", _BaseColor);
        }
    }
}

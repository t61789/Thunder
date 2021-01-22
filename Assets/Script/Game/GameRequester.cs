using Framework;
using Thunder.UI;

using UnityEngine;

namespace Thunder.Game
{
    public class GameRequester : BaseEntity, IHitAble
    {
        private Color _BaseColor;

        private Material _Mat;
        private bool _Requested;
        private SimpleCounter _ReRequestSimpleCounter;

        private bool _Started;
        public GameType GameType;
        public Color RequestedColor = Color.red;
        public float ReRequestTime = 3;

        public void GetHit(Vector3 hitPos, Vector3 hitDir, float damage)
        {
            if (!_ReRequestSimpleCounter.Completed || _Started) return;

            _Requested = !_Requested;
            _Mat.SetColor("_Light", _Requested ? RequestedColor : _BaseColor);
            PublicEvents.GameRequest?.Invoke(GameType, _Requested);
            _ReRequestSimpleCounter.Recount();
            LogPanel.Ins.LogSystem(_Requested
                ? TextSys.RequestGameSuccess
                : TextSys.CancelGameRequest);
        }

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

        private void GameEnd(GameType type, bool completely)
        {
            if (type != GameType) return;
            _Requested = false;
            _Started = false;
            _Mat.SetColor("_Light", _BaseColor);
        }
    }
}
using Thunder.Tool;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Game.FlyingSaucer
{
    public class FlyingSaucerSwitcher:MonoBehaviour,IShootable
    {
        public float ReRequestTime = 3;
        public Color RequestedColor = Color.red;

        private bool _Started;
        private bool _Requested;
        private SimpleCounter _ReRequestSimpleCounter;
        private Color _BaseColor;

        private Material _Mat;

        private void Awake()
        {
            PublicEvents.FlyingSaucerGameStart.AddListener(()=>_Started=true);
            PublicEvents.FlyingSaucerGameEnd.AddListener(GameEnd);
            _ReRequestSimpleCounter = new SimpleCounter(ReRequestTime,false);
            _Mat = GetComponent<MeshRenderer>().StandaloneMaterial();
            _BaseColor = _Mat.GetColor("_Light");
        }

        public void GetShoot(Vector3 hitPos, Vector3 hitDir, float damage)
        {
            if (!_ReRequestSimpleCounter.Completed || _Started) return;

            _Requested = !_Requested;
            _Mat.SetColor("_Light", _Requested?RequestedColor:_BaseColor);
            PublicEvents.FlyingSaucerGameRequest?.Invoke(_Requested);
            _ReRequestSimpleCounter.Recount();
            LogPanel.Instance.LogSystem(_Requested
                ? "Please stand on the start point to start the game"
                : "You have canceled the game");
        }

        private void GameEnd(bool completely)
        {
            _Requested = false;
            _Started = false;
            _Mat.SetColor("_Light", _BaseColor);
        }
    }
}

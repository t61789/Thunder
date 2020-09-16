using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Entity;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Game.FlyingSaucer
{
    public class FlyingSaucerShootPoint:AreaTrigger
    {
        private bool _Requested;
        private bool _Started;
        //todo 中途离开
        private void Awake()
        {
            PublicEvents.FlyingSaucerGameRequest.AddListener(x=>_Requested=x);
            PublicEvents.FlyingSaucerGameEnd.AddListener(GameEnd);
        }

        protected override void Stay(Collider collider)
        {
            if (!_Requested || _Started)
                return;

            _Started = true;

            PublicEvents.FlyingSaucerGameStartDelay?.Invoke();

            Player.Instance.Movable = false;
        }

        private void GameEnd(bool completely)
        {
            _Requested = false;
            _Started = false;

            Player.Instance.Movable = true;
        }
    }
}

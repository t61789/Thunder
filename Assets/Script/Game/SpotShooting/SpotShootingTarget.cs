using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Entity;
using Thunder.Tool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Game.SpotShooting
{
    public class SpotShootingTarget:BaseEntity,IShootable
    {
        public bool Rised = false;
        public float RiseTime = 0.7f;
        public float RiseAngle = 0;
        public float UnRiseAngle = -90;
        public float LifeTime = 2;

        private bool _RotateToRise;
        private Collider _Collider;
        private AutoCounter _RiseCounter;
        private AutoCounter _LifeCounter;

        protected override void Awake()
        {
            base.Awake();
            _Collider = GetComponent<Collider>();
            _Collider.enabled = false;
            _RiseCounter = new AutoCounter(this,RiseTime);
            _LifeCounter = new AutoCounter(this,LifeTime,false).OnComplete(UnRise);
        }

        private void FixedUpdate()
        {
            var rot = _Trans.eulerAngles;
            rot.x = Tools.Lerp(_RotateToRise ? UnRiseAngle : RiseAngle,
                _RotateToRise ? RiseAngle : UnRiseAngle, _RiseCounter.Interpolant);
            _Trans.eulerAngles = rot;
        }

        public void Rise()
        {
            if (Rised) return;
            Rised = true;
            _RotateToRise = true;
            _RiseCounter.OnComplete(RiseCompleted).Recount();
        }

        private void RiseCompleted()
        {
            _Collider.enabled = true;
            _LifeCounter.Resume();
        }

        public void UnRise()
        {
            if (!_Collider.enabled) return;

            _Collider.enabled = false;
            _RotateToRise = false;
            _RiseCounter.OnComplete(UnRiseCompleted).Recount();
        }

        private void UnRiseCompleted()
        {
            Rised = false;
            _LifeCounter.Pause().Recount();
        }

        public void GetShoot(Vector3 hitPos, Vector3 hitDir, float damage)
        {
            PublicEvents.SpotShootingTargetHit?.Invoke(this);
            
        }
    }
}

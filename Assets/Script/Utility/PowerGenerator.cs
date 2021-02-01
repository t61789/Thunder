using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FairyGUI;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class PowerGenerator:BaseBuilding
    {
        public float PowerPerSec = 10;
        public float UpdateTime = 0.5f;

        private SimpleCounter _UpdateCounter;

        protected override void Awake()
        {
            base.Awake();
            _UpdateCounter = new SimpleCounter(UpdateTime);
        }

        private void Update()
        {
            if (_UpdateCounter.Completed)
            {
                _UpdateCounter.Recount();
                GlobalResource.Ins.Power.Add(PowerPerSec * UpdateTime);
            }
        }
    }
}

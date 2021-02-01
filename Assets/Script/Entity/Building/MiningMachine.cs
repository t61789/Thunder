using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using Thunder;
using UnityEngine;

namespace Thunder
{
    public class MiningMachine:BaseBuilding,IInteractive
    {
        public int Capacity = 10;
        public float ProduceTime = 10;
        public ItemGroup Product;

        private CommonPackage _Package;
        private SimpleCounter _ProduceCounter;
        private Dropper _Dropper;

        protected override void Awake()
        {
            _Package = new CommonPackage(Capacity);
            _ProduceCounter = new SimpleCounter(ProduceTime);
            _Dropper = new Dropper(Config.DefaultDropForce, () => Trans.position, () => Trans.rotation);
        }

        private void FixedUpdate()
        {
            if (_ProduceCounter.Completed)
            {
                int remaining = _Package.PutItem(Product).Remaining.FirstOrDefault().Count;
                Debug.Log(_Package.GetItemStr());
                if (remaining != 0)
                {
                    _Dropper.Drop(new ItemGroup(Product.Id, remaining));
                    Debug.Log("drop");
                }
                _ProduceCounter.Recount();
            }
        }

        public void Interactive(ControlInfo info)
        {
            
        }
    }
}

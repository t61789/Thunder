using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity
{
    public class BulletHoleManager : MonoBehaviour
    {
        public static BulletHoleManager Instance;

        public float ClearTime=10;
        public int HolesLimit=20;
        public Sprite[] Sprites;

        private CircleBuffer<BulletHole> _BulletHoles;
        private Counter _ClearCounter;

        private void Awake()
        {
            Instance = this;
            _BulletHoles = new CircleBuffer<BulletHole>(HolesLimit);
            _ClearCounter = new Counter(ClearTime).OnComplete(Clear).ToAutoCounter(this);
        }

        public static void Create(Vector3 pos,Vector3 normal)
        {
            BulletHole hole = ObjectPool.Ins.Alloc<BulletHole>("bulletHole",x=>x.Init(pos,normal,Instance.Sprites.RandomTake()));
            hole= Instance._BulletHoles.Insert(hole);
            if(hole!=null)
                ObjectPool.Ins.Recycle(hole);
        }

        private void Clear()
        {
            BulletHole hole = _BulletHoles.Remove();
            if (hole != null)
                ObjectPool.Ins.Recycle(hole);
            _ClearCounter.Recount();
        }
    }
}

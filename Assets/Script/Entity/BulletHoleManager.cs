using System.Collections.Generic;
using Framework;
using UnityEngine;

namespace Thunder.Entity
{
    public class BulletHoleManager : MonoBehaviour
    {
        public static BulletHoleManager Instance;

        private PipelineQueue<BulletHole> _BulletHoles;
        private AutoCounter _ClearCounter;

        public float ClearTime = 10;
        public int HolesLimit = 20;
        public Sprite[] Sprites;

        private void Awake()
        {
            Instance = this;
            _BulletHoles = new PipelineQueue<BulletHole>(HolesLimit);
            _ClearCounter = new AutoCounter(this, ClearTime).OnComplete(Clear);
        }

        public static void Create(Vector3 pos, Vector3 normal)
        {
            var hole = ObjectPool.Get<BulletHole>("bulletHole");
            hole.Init(pos, normal, Instance.Sprites.RandomTake());
            hole = Instance._BulletHoles.Enqueue(hole);
            if (hole != null)
                ObjectPool.Put(hole);
        }

        private void Clear()
        {
            var hole = _BulletHoles.Dequeue();
            if (hole != null)
                ObjectPool.Put(hole);
            _ClearCounter.Recount();
        }
    }
}
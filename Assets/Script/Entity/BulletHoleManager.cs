using Thunder.Tool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity
{
    public class BulletHoleManager : MonoBehaviour
    {
        public static BulletHoleManager Instance;

        private CircleQueue<BulletHole> _BulletHoles;
        private AutoCounter _ClearCounter;

        public float ClearTime = 10;
        public int HolesLimit = 20;
        public Sprite[] Sprites;

        private void Awake()
        {
            Instance = this;
            _BulletHoles = new CircleQueue<BulletHole>(HolesLimit);
            _ClearCounter = new AutoCounter(this, ClearTime).OnComplete(Clear);
        }

        public static void Create(Vector3 pos, Vector3 normal)
        {
            var hole = ObjectPool.Ins.Alloc<BulletHole>("bulletHole");
            hole.Init(pos, normal, Instance.Sprites.RandomTake());
            hole = Instance._BulletHoles.Enqueue(hole);
            if (hole != null)
                ObjectPool.Ins.Recycle(hole);
        }

        private void Clear()
        {
            var hole = _BulletHoles.Dequeue();
            if (hole != null)
                ObjectPool.Ins.Recycle(hole);
            _ClearCounter.Recount();
        }
    }
}
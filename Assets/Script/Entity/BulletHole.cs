using Framework;

using UnityEngine;

namespace Thunder
{
    public class BulletHole : BaseEntity, IObjectPool
    {
        private SpriteRenderer _SpriteRenderer;

        public AssetId AssetId { get; set; }

        public void OpReset()
        {
        }

        public void OpPut()
        {
        }

        public void OpDestroy()
        {
        }

        protected override void Awake()
        {
            base.Awake();
            _SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Init(Vector3 pos, Vector3 normal, Sprite sprite)
        {
            Trans.position = pos;
            Trans.rotation = Quaternion.LookRotation(normal);
            _SpriteRenderer.sprite = sprite;
        }
    }
}
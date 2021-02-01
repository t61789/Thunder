using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class CommonMuzzleFire:BaseEntity,IObjectPool
    {
        public float LifeTime = 0.1f;
        public Sprite[] Textures;

        private SimpleCounter _LifeTimeCounter;
        private SpriteRenderer _SpriteRenderer;

        protected override void Awake()
        {
            base.Awake();
            _LifeTimeCounter = new SimpleCounter(LifeTime);
            _SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            if (_LifeTimeCounter.Completed)
            {
                Hide();
            }
        }

        public void Init(Transform parent,Vector3 localPos,Quaternion localRot)
        {
            Trans.SetParent(parent);
            Trans.localPosition = localPos;
            Trans.localRotation = localRot;
            Trans.localScale = Vector3.one;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _LifeTimeCounter.Recount();
            _SpriteRenderer.sprite = Textures.RandomTake();
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }

        public AssetId AssetId { get; set; }

        public void OpReset() { }

        public void OpPut() { }

        public void OpDestroy() { }
    }
}

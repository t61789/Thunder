using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Sys;
using Thunder.Tool.ObjectPool;
using UnityEngine;

namespace Thunder.Entity
{
    public class BulletHole:BaseEntity,IObjectPool
    {
        private SpriteRenderer _SpriteRenderer;

        protected override void Awake()
        {
            base.Awake();
            _SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Init(Vector3 pos, Vector3 normal,Sprite sprite)
        {
            _Trans.position = pos;
            _Trans.rotation = Quaternion.LookRotation(normal);
            _SpriteRenderer.sprite = sprite;
        }

        public AssetId AssetId { get; set; }
        public void OpReset()
        {
            
        }

        public void OpRecycle()
        {
        }

        public void OpDestroy()
        {
        }
    }
}

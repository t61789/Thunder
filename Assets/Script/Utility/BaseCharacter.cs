using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class BaseCharacter:BaseEntity
    {
        public float MaxHealth = 100;

        public NumericResource Health;

        public string Camp = "neutral";

        protected override void Awake()
        {
            base.Awake();
            Health = new NumericResource(0, MaxHealth, MaxHealth);
            Health.ReachMin += Dead;
        }

        public virtual void GetHit(Vector3 hitPos, Vector3 hitDir, float damage) { }

        protected virtual void Dead() { }
    }
}

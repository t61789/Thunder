using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thunder.Entity
{
    public abstract class PickupableItem:BaseEntity
    {
        public abstract void Pickup(Collider collider);

        private void OnTriggerEnter(Collider collider)
        {
            Pickup(collider);
        }


    }
}

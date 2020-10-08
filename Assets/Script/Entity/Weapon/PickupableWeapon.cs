using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thunder.Entity.Weapon
{
    public class PickupableWeapon:PickupableItem
    {
        public override void Pickup(Collider collider)
        {
            var player = collider.GetComponent<Player>();
            if (player == null) return;

            
        }
    }
}

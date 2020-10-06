using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Entity.Weapon;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity
{
    public class AmmoCase:AreaTrigger
    {
        protected override void Enter(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (player == null || BaseWeapon.Ins==null)
                return;
            BaseWeapon.Ins.FillAmmo();
        }
    }
}

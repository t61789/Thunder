using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity
{
    public class AmmoCase:AreaTrigger
    {
        protected override void Enter(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (player == null || Gun.Instance==null)
                return;
            Gun.Instance.FillAmmo();
        }
    }
}

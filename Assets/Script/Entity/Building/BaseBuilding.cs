using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class BaseBuilding:BaseCharacter
    {
        public int BuildingId;

        public virtual void Install(Vector3 pos,Quaternion rot)
        {
            Trans = transform;
            Trans.position = pos;
            Trans.rotation = rot;
        }
    }
}

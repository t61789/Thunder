using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thunder.Utility
{
    [DontGenerateWrap]
    public abstract class BaseCamera : MonoBehaviour
    {
        protected Transform Trans;
        protected virtual void Awake()
        {
            Trans = transform;
        }
    }
}

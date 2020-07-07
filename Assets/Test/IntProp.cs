using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Test
{
    public class IntProp:MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log(transform.localToWorldMatrix.MultiplyVector(Vector2.one));
        }
    }
}

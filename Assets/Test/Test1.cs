using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Test
{
    public class Test1:MonoBehaviour
    {
        public GameObject go;

        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 700, 500), "fff"))
            {
                Time.timeScale = 0;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Thunder.Utility
{
    public class AreaTrigger:MonoBehaviour
    {
        public UnityEvent<Collider> Enter;
        public UnityEvent<Collider> Stay;
        public UnityEvent<Collider> Exit;

        private void OnTriggerEnter(Collider collider)
        {
            Enter?.Invoke(collider);
        }

        private void OnTriggerStay(Collider collider)
        {
            Stay?.Invoke(collider);
        }

        private void OnTriggerExit(Collider collider)
        {
            Exit?.Invoke(collider);
        }
    }
}

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
        public UnityEvent<Collider> OnEnter;
        public UnityEvent<Collider> OnStay;
        public UnityEvent<Collider> OnExit;

        protected virtual void OnTriggerEnter(Collider collider)
        {
            Enter(collider);
            OnEnter?.Invoke(collider);
        }

        protected virtual void OnTriggerStay(Collider collider)
        {
            Stay(collider);
            OnStay?.Invoke(collider);
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
            Exit(collider);
            OnExit?.Invoke(collider);
        }

        protected virtual void Enter(Collider collider)
        {

        }

        protected virtual void Stay(Collider collider)
        {
            
        }

        protected virtual void Exit(Collider collider)
        {

        }
    }
}

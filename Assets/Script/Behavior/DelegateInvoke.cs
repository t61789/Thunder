using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace Thunder.Behavior
{
    public class DelegateInvoke : Action
    {
        private NoArgMethod method;

        public string methodName;
        public SharedObject sharedObject;

        public override TaskStatus OnUpdate()
        {
            try
            {
                if (method == null)
                    method = (NoArgMethod) Delegate.CreateDelegate(typeof(NoArgMethod), sharedObject.Value, methodName);

                return method();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return TaskStatus.Failure;
            }
        }

        private delegate TaskStatus NoArgMethod();
    }
}
using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Thunder.Behavior
{
    public class DelegateInvoke : BehaviorDesigner.Runtime.Tasks.Action
    {
        public SharedObject sharedObject;

        public string methodName;

        private delegate TaskStatus NoArgMethod();
        private NoArgMethod method;

        public override TaskStatus OnUpdate()
        {
            try
            {
                if (method == null)
                    method = (NoArgMethod)Delegate.CreateDelegate(typeof(NoArgMethod), sharedObject.Value, methodName);

                return method();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return TaskStatus.Failure;
            }
        }
    }
}

using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace Thunder.Behavior
{
    public class DelegateFixedInvoke : Action
    {
        private bool fixedUpdating = true;
        private NoArgMethod method;

        public string methodName;
        public SharedObject sharedObject;

        public override TaskStatus OnUpdate()
        {
            if (!fixedUpdating)
            {
                fixedUpdating = true;
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        public override void OnFixedUpdate()
        {
            try
            {
                if (method == null)
                    method = (NoArgMethod) Delegate.CreateDelegate(typeof(NoArgMethod), sharedObject.Value, methodName);

                method();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            fixedUpdating = false;
        }

        private delegate void NoArgMethod();
    }
}
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;

namespace Thunder.Behavior
{
    public class DelegateFixedInvoke : BehaviorDesigner.Runtime.Tasks.Action
    {
        public SharedObject sharedObject;

        public string methodName;

        private bool fixedUpdating = true;

        private delegate void NoArgMethod();
        private NoArgMethod method;

        public override TaskStatus OnUpdate()
        {
            if (!fixedUpdating)
            {
                fixedUpdating = true;
                return TaskStatus.Success;
            }
            else
                return TaskStatus.Running;
        }

        public override void OnFixedUpdate()
        {
            try
            {
                if (method == null)
                    method = (NoArgMethod)Delegate.CreateDelegate(typeof(NoArgMethod), sharedObject.Value, methodName);

                method();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            fixedUpdating = false;
        }
    }
}

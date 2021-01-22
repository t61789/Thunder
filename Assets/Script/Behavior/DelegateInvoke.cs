using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace Thunder.Behavior
{
    public class DelegateInvoke : Action
    {
        private NoArgMethod _Method;

        public string MethodName;
        public SharedObject Component;

        public override TaskStatus OnUpdate()
        {
            try
            {
                if (_Method == null)
                    _Method = CreateDelegate();

                return _Method();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return TaskStatus.Failure;
            }
        }

        private delegate TaskStatus NoArgMethod();

        private NoArgMethod CreateDelegate()
        {
            return (NoArgMethod)Delegate.CreateDelegate(typeof(NoArgMethod), Component.Value, MethodName);
        }

        public void TestProperties()
        {
            var hasError = false;
            if (Component.Value == null)
            {
                Debug.LogError($"[{FriendlyName}] Component is null");
                hasError = true;
            }

            if (string.IsNullOrEmpty(MethodName))
            {
                Debug.LogError($"[{FriendlyName}] MethodName is null or empty");
                hasError = true;
            }

            if (hasError) return;

            try
            {
                var a = (NoArgMethod) Delegate.CreateDelegate(typeof(NoArgMethod), Component.Value, MethodName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{FriendlyName}] {e}");
            }
        }
    }
}
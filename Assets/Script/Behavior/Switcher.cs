using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Thunder.Behavior
{
    public class Switcher : Decorator
    {
        private TaskStatus executionStatus = TaskStatus.Inactive;
        private NoArgMethod failedMethod;
        public string failedMethodName;
        private TaskStatus preTaskStatus = TaskStatus.Failure;

        public SharedObject sharedObject;
        private NoArgMethod successMethod;
        public string successMethodName;

        public override bool CanExecute()
        {
            return executionStatus == TaskStatus.Inactive || executionStatus == TaskStatus.Running;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            executionStatus = childStatus;
        }

        public override TaskStatus Decorate(TaskStatus status)
        {
            try
            {
                if (successMethodName != null && successMethodName != "" && status == TaskStatus.Success &&
                    preTaskStatus == TaskStatus.Failure)
                {
                    if (successMethod == null)
                        successMethod = (NoArgMethod) Delegate.CreateDelegate(typeof(NoArgMethod), sharedObject.Value,
                            successMethodName);

                    successMethod();
                }
                else if (failedMethodName != null && failedMethodName != "" && status == TaskStatus.Failure &&
                         preTaskStatus == TaskStatus.Success)
                {
                    if (failedMethod == null)
                        failedMethod = (NoArgMethod) Delegate.CreateDelegate(typeof(NoArgMethod), sharedObject.Value,
                            failedMethodName);

                    failedMethod();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return TaskStatus.Failure;
            }

            preTaskStatus = status;

            return status;
        }

        public override void OnEnd()
        {
            executionStatus = TaskStatus.Inactive;
        }

        private delegate TaskStatus NoArgMethod();
    }
}
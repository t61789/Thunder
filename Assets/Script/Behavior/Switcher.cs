using System;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Thunder.Behavior
{
    public class Switcher : Decorator
    {
        public bool StartValue;
        public bool SuccessWhenToSuccess;
        public bool SuccessWhenToFailure;

        private TaskStatus _PreTaskStatus;
        private TaskStatus _ExecutionStatus = TaskStatus.Inactive;

        public override void OnAwake()
        {
            base.OnAwake();
            _PreTaskStatus = StartValue ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override bool CanExecute()
        {
            return _ExecutionStatus == TaskStatus.Inactive || _ExecutionStatus == TaskStatus.Running;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            _ExecutionStatus = childStatus;
        }

        public override TaskStatus Decorate(TaskStatus status)
        {
            try
            {
                if (SuccessWhenToSuccess &&
                    _PreTaskStatus == TaskStatus.Failure &&
                    status == TaskStatus.Success ||

                    SuccessWhenToFailure &&
                    _PreTaskStatus == TaskStatus.Success &&
                    status == TaskStatus.Failure)
                {
                    _PreTaskStatus = status;
                    return TaskStatus.Success;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return TaskStatus.Failure;
            }

            return TaskStatus.Failure;
        }

        public override void OnEnd()
        {
            _ExecutionStatus = TaskStatus.Inactive;
            
        }
    }
}
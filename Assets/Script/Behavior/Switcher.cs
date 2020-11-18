using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Thunder.Behavior
{
    public class Switcher : Decorator
    {
        public bool SuccessWhenToSuccess;
        public bool SuccessWhenToFailure;

        private TaskStatus _PreTaskStatus = TaskStatus.Failure;
        private TaskStatus _ExecutionStatus = TaskStatus.Inactive;

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
                    status == TaskStatus.Success||

                    SuccessWhenToFailure && 
                    _PreTaskStatus == TaskStatus.Success && 
                    status == TaskStatus.Failure)
                    return TaskStatus.Success;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return TaskStatus.Failure;
            }

            _PreTaskStatus = status;

            return status;
        }

        public override void OnEnd()
        {
            _ExecutionStatus = TaskStatus.Inactive;
        }
    }
}
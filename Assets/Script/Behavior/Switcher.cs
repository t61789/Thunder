using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;

public class Switcher : Decorator
{
    private TaskStatus preTaskStatus = TaskStatus.Failure;

    public SharedObject sharedObject;
    public string successMethodName;
    public string failedMethodName;

    private delegate TaskStatus NoArgMethod();
    private NoArgMethod successMethod;
    private NoArgMethod failedMethod;

    private TaskStatus executionStatus = TaskStatus.Inactive;

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
            if (successMethodName != null && successMethodName != "" && status == TaskStatus.Success && preTaskStatus == TaskStatus.Failure)
            {
                if (successMethod == null)
                    successMethod = (NoArgMethod)Delegate.CreateDelegate(typeof(NoArgMethod), sharedObject.Value, successMethodName);

                successMethod();
            }
            else if (failedMethodName != null && failedMethodName != "" && status == TaskStatus.Failure && preTaskStatus == TaskStatus.Success)
            {
                if (failedMethod == null)
                    failedMethod = (NoArgMethod)Delegate.CreateDelegate(typeof(NoArgMethod), sharedObject.Value, failedMethodName);

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
}

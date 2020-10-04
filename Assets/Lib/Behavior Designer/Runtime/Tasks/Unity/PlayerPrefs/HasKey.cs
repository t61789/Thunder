using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityPlayerPrefs
{
    [TaskCategory("Unity/PlayerPrefs")]
    [TaskDescription("Retruns success if the specified Key exists.")]
    public class HasKey : Conditional
    {
        [Tooltip("The Key to check")]
        public SharedString key;

        public override TaskStatus OnUpdate()
        {
            return PlayerPrefs.HasKey(key.Value) ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            key = "";
        }
    }
}
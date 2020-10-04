using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityPlayerPrefs
{
    [TaskCategory("Unity/PlayerPrefs")]
    [TaskDescription("Sets the value with the specified Key from the PlayerPrefs.")]
    public class SetInt : Action
    {
        [Tooltip("The Key to store")]
        public SharedString key;
        [Tooltip("The value to set")]
        public SharedInt value;

        public override TaskStatus OnUpdate()
        {
            PlayerPrefs.SetInt(key.Value, value.Value);

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            key = "";
            value = 0;
        }
    }
}
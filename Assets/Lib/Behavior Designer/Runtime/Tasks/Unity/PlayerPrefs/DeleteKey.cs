using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityPlayerPrefs
{
    [TaskCategory("Unity/PlayerPrefs")]
    [TaskDescription("Deletes the specified Key from the PlayerPrefs.")]
    public class DeleteKey : Action
    {
        [Tooltip("The Key to delete")]
        public SharedString key;

        public override TaskStatus OnUpdate()
        {
            PlayerPrefs.DeleteKey(key.Value);

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            key = "";
        }
    }
}
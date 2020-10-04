using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityPlayerPrefs
{
    [TaskCategory("Unity/PlayerPrefs")]
    [TaskDescription("Stores the value with the specified Key from the PlayerPrefs.")]
    public class GetInt : Action
    {
        [Tooltip("The Key to store")]
        public SharedString key;
        [Tooltip("The default value")]
        public SharedInt defaultValue;
        [Tooltip("The value retrieved from the PlayerPrefs")]
        [RequiredField]
        public SharedInt storeResult;

        public override TaskStatus OnUpdate()
        {
            storeResult.Value = PlayerPrefs.GetInt(key.Value, defaultValue.Value);

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            key = "";
            defaultValue = 0;
            storeResult = 0;
        }
    }
}
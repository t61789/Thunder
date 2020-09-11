using UnityEngine;

namespace Thunder.Utility
{
    [DontGenerateWrap]
    public abstract class BaseCamera : MonoBehaviour
    {
        protected Transform Trans;
        protected virtual void Awake()
        {
            Trans = transform;
        }
    }
}

using UnityEngine;

namespace Assets.Test
{
    public class IntProp : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log(transform.localToWorldMatrix.MultiplyVector(Vector2.one));
        }
    }
}

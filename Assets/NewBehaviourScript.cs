using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 400, 100), "hit"))
        {
            Debug.Log(1);
            Matrix4x4 mvp = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix;
            foreach (var raycastHit in Physics.SphereCastAll(Vector3.zero, 10, Vector3.one))
            {
                Vector4 result = mvp * raycastHit.transform.position;
                //float w = Mathf.Abs(result.w);
                Debug.Log(raycastHit.transform.name);
                bool x = result.x <= result.w && result.x >= -result.w;
                bool y = result.y <= result.w && result.y >= -result.w;
                bool z = result.z <= result.w && result.z >= -result.w;
                Debug.Log($"{x} {y} {z}");
                if (x && y)
                {
                    
                    Debug.Log(result/result.w);
                    //Debug.Log(result);
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Thunder.Tool;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public Transform target;
    public Vector2 Uvm;
    public float Depth;

    private void FixedUpdate()
    {
        Gizmos.color = Color.blue;
        Vector4 middle = new Vector4(Uvm.x, Uvm.y, Depth, 1);
        middle = (Camera.main.projectionMatrix*Camera.main.worldToCameraMatrix).inverse * middle;
        middle /= middle.w;
        target.position = middle.Xyz();
    }
}

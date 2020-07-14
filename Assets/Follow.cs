using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform Mover;
    public Vector3 Offset;

    public bool Normal;
    public bool Fixed;
    public bool Late;
    public bool Lerp;
    public float SmoothFactor;

    private Vector3 TargetPos;
    private Time timeLerp;

    private Vector3[] Points;

    private void Calc()
    {
        float o1 = 0;
        float o1Step = 1;
        float o2 = 0;
        float factor = 0.2f;
        float loop = 100;

        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < loop; i++)
        {
            float delta = (o1 - o2) * factor;
            Debug.Log(delta);
            o2 += delta;
            o1 += o1Step;
            result.Add(new Vector3(o1,o2));
        }

        Points = result.ToArray();

    }

    private void OnDrawGizmos()
    {
        if (Points == null) return;
        Vector3 start = Vector3.zero;
        foreach (var vector3 in Points)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(start,vector3);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(vector3, new Vector3(vector3.x, vector3.x));
            start = vector3;
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 500, 250), "Calc"))
        {
            Calc();
        }
    }

    private void Awake()
    {
        //Calc();
        
    }

    private void Update()
    {
        
        if (Normal)
            Next();
    }

    private void FixedUpdate()
    {
        if (Fixed)
            Next();
    }

    private void LateUpdate()
    {
        if (Late)
            Next();

        //transform.position = Vector3.Lerp(transform.position,TargetPos, SmoothFactor); ;

        //transform.position = TargetPos;

        //Vector3 temp = default;
        //transform.position = Vector3.SmoothDamp(transform.position, TargetPos, ref temp, SmoothFactor);
    }

    private void Next()
    {
        if (Lerp)
        {
            float distance = Vector3.Distance(transform.position, Mover.position + Offset);
            transform.position = Vector3.MoveTowards(transform.position, Mover.position + Offset, distance * SmoothFactor);
        }
        else
            transform.position = Mover.position + Offset;
    }
}

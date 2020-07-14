using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float Speed;
    public float Offset;

    public bool Normal;
    public bool Fixed;
    public bool Late;

    void Start()
    {
        
    }

    void Update()
    {
        if(Normal)
            Next();
    }

    void FixedUpdate()
    {
        if (Fixed)
            Next();
    }

    void LateUpdate()
    {
        if (Late)
            Next();
    }

    private void Next()
    {
        //transform.position = new Vector3(Mathf.Cos(Time.time * Speed), 0, Mathf.Sin(Time.time * Speed)) + Vector3.up * 3;
        //transform.position += Vector3.right * 0.01f * Time.time;
        transform.position = new Vector3(Time.time * Speed, Offset);
    }
}

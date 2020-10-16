using System;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Test1 t;


}

[Serializable]
public class Test1
{
    public int A;

    public Test1(int a)
    {
        Debug.Log(a);
        A = a;
    }
}

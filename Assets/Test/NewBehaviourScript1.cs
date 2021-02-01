using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

public class NewBehaviourScript1 : MonoBehaviour
{
    public static event Action Say;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(500, 0, 300, 200), "123"))
        {
            Destroy(GameObject.Find("Shit"));
        }

        if (GUI.Button(new Rect(500, 200, 300, 200), "12222"))
        {
            Say?.Invoke();
        }
    }
}

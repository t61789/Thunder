using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using FairyGUI;
using Framework;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200, 100), "Test"))
        {
            UiSys.OpenUi("packagePanel");
        }
    }
}

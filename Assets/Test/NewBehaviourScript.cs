using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using FairyGUI;
using Framework;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    private void Start()
    {
        var btn = GetComponent<UIPanel>().ui.GetChild("n0").asButton;
        btn.onClick.Add(()=>Debug.Log(123));

    }

    private void Update()
    {
    }
}

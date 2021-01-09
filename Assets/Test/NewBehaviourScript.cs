using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private BehaviorTree _Tree;

    private void Awake()
    {
        _Tree = GetComponent<BehaviorTree>();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 300, 200), "123"))
        {
            _Tree.SetVariableValue("Check",false);
        }
    }
}

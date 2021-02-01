using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using FairyGUI;
using Framework;
using Newtonsoft.Json;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private Shit Ss;

    private void Awake()
    {
        Ss = new Shit();
        NewBehaviourScript1.Say += Ss.Say;
    }

    private class Shit
    {
        public void Say()
        {
            Debug.Log(123);
        }
    }
}

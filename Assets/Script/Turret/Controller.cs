﻿using UnityEngine;

public class Controller : MonoBehaviour
{

    public string ControllerName
    {
        set
        {
            _ControllerName = value;
        }

        get
        {
            if (_ControllerName == null || _ControllerName == "")
                return name;
            else
                return _ControllerName;
        }
    }

    public string InputId
    {
        set
        {
            _InputId = value;
        }

        get
        {
            if (_InputId == null)
                return name;
            else
                return _InputId;
        }
    }

    [SerializeField]
    private string _InputId;
    [SerializeField]
    private string _ControllerName;

    protected virtual void Awake()
    {

    }
}

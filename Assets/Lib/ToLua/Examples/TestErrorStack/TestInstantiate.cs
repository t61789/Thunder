﻿using LuaInterface;
using System;
using UnityEngine;

public class TestInstantiate : MonoBehaviour
{
    void Awake()
    {
        LuaState state = LuaState.Get(IntPtr.Zero);

        try
        {
            LuaFunction func = state.GetFunction("Show");
            func.BeginPCall();
            func.PCall();
            func.EndPCall();
            func.Dispose();
            func = null;
        }
        catch (Exception e)
        {
            state.ThrowLuaException(e);
        }
    }

    void Start()
    {
        Debugger.Log("start");
    }
}

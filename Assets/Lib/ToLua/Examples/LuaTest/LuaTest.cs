using LuaInterface;
using System;
using UnityEngine;

public class LuaTest : MonoBehaviour
{
    public LuaState _Ls;

    public static LuaTest instance;

    private void Awake()
    {
        _Ls = new LuaState();
        _Ls.Start();
        _Ls.AddSearchPath(Application.dataPath + "/Test/Lua");

        LuaBinder.Bind(_Ls);
        DelegateFactory.Init();

        instance = this;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 50), "File1"))
        {
            _Ls.DoString(
@"s = Thunder.Test.Shit()
s.SetNormal(s,'222')
print(s.normal)");
        }
    }

    private void OnApplicationQuit()
    {
        _Ls.Dispose();
        _Ls = null;
    }
}


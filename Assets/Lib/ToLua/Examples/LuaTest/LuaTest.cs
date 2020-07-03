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
            _Ls.DoFile("File1");

            (_Ls["shit"] as Shit).fuck(1);
        }
    }

    private void OnApplicationQuit()
    {
        _Ls.Dispose();
        _Ls = null;
    }
}

public class Shit
{
    public Func<int, int> fuck;
}

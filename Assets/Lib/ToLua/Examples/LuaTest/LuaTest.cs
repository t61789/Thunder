using System;
using LuaInterface;
using UnityEngine;

public class LuaTest : MonoBehaviour
{
    private Yeah a;

    private void Awake()
    {
        a = new Yeah();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 50), "File1"))
        {
            a._Ls.DoFile("File1");


            Action fuc = a._Ls.GetFunction("Fuck").ToDelegate<Action>();
            fuc();
        }

        if (GUI.Button(new Rect(0, 50, 100, 50), "File2"))
        {
            a._Ls.DoFile("File2");
        }

        if (GUI.Button(new Rect(0, 100, 100, 50), "Loada"))
        {
            Debug.Log(a._Ls["a"]);
        }

        if (GUI.Button(new Rect(0, 150, 100, 50), "SetNull"))
        {
            a._Ls["a"] = null;
        }

        if (GUI.Button(new Rect(0, 200, 100, 50), "Dispose"))
        {
            a._Ls.Dispose();
        }

        if (GUI.Button(new Rect(0, 250, 100, 50), "Start"))
        {
            a._Ls.Start();
        }

        //a._Ls.CheckTop();
        //a._Ls.Collect();
    }

    private void OnApplicationQuit()
    {
        a._Ls.Dispose();
        a._Ls = null;
    }
}

public class Yeah
{
    public LuaState _Ls;

    public Yeah()
    {
        Awake();
    }

    public void Awake()
    {
        _Ls = new LuaState();
        _Ls.Start();
        _Ls.AddSearchPath(Application.dataPath + "/Test/Lua");

        LuaBinder.Bind(_Ls);
        DelegateFactory.Init();
    }
}

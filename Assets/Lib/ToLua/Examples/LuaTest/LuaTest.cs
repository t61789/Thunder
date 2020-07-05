using LuaInterface;
using System;
using Thunder.Sys;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

public class LuaTest : MonoBehaviour
{
    private void Awake()
    { 
        //bs.luaState = Stable.Lua.LuaState;
    }

    private void OnGUI()
    {
//        if (GUI.Button(new Rect(0, 0, 100, 50), "Load"))
//        {
//            string str =
//@"function ReceiveSoldiers()
//    soldiers = System.Collections.Generic.List_Thunder_Entity_Person()
//    for id=0,300,1 do
//        soldiers:Add(Thunder.Entity.Person(id))
//    end
//    return soldiers
//end";

//            Stable.Lua.ExecuteCommand(str);
//        }

//        if (GUI.Button(new Rect(0, 50, 100, 50), "Rec"))
//        {
//            bs.ReceiveSoldiers();
//        }
    }

    public void Exec(string cmd)
    {
        
        //Thunder.Sys.Stable.Lua.ExecuteCommand(cmd);
    }
}


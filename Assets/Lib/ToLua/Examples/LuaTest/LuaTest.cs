using LuaInterface;
using System;
using Thunder.Sys;
using UnityEngine;

public class LuaTest : MonoBehaviour
{
    private void Awake()
    {
        //bs.luaState = Stable.Lua.LuaState;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 50), "Load"))
        {
            string str =
@"TestScope = {}
TestScope.Read = function(ins)
    ins = ins + 1
end";

            for (int j = 0; j < 3; j++)
            {
                const int loop = 1000000;
                Stable.Lua.ExecuteCommand(str);
                LuaTable table = Stable.Lua.LuaState["TestScope"] as LuaTable;
                LuaFunction func = table.GetLuaFunction("Read");

                float time = Time.realtimeSinceStartup;
                for (int i = 0; i < loop; i++)
                {
                    Stable.Lua.LuaState.Call("TestScope.Read", 1, true);
                }
                Debug.Log($"From state:{Time.realtimeSinceStartup - time}");
                time = Time.realtimeSinceStartup;

                for (int i = 0; i < loop; i++)
                {
                    table.Call("Read", 1);
                }
                Debug.Log($"From table:{Time.realtimeSinceStartup - time}");
                time = Time.realtimeSinceStartup;

                for (int i = 0; i < loop; i++)
                {
                    func.Call(1);
                }
                Debug.Log($"From luafunction:{Time.realtimeSinceStartup - time}");
                time = Time.realtimeSinceStartup;

                Action<int> act = func.ToDelegate<Action<int>>();
                for (int i = 0; i < loop; i++)
                {
                    act(1);
                }
                Debug.Log($"From delegate:{Time.realtimeSinceStartup - time}");
                time = Time.realtimeSinceStartup;
            }
        }
    }
}


﻿//this source code was auto-generated by tolua#, do not modify it
using LuaInterface;
using System;

public class UnityEngine_AudioBehaviourWrap
{
    public static void Register(LuaState L)
    {
        L.BeginClass(typeof(UnityEngine.AudioBehaviour), typeof(UnityEngine.Behaviour));
        L.RegFunction("New", _CreateUnityEngine_AudioBehaviour);
        L.RegFunction("__eq", op_Equality);
        L.RegFunction("__tostring", ToLua.op_ToString);
        L.EndClass();
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int _CreateUnityEngine_AudioBehaviour(IntPtr L)
    {
        try
        {
            int count = LuaDLL.lua_gettop(L);

            if (count == 0)
            {
                UnityEngine.AudioBehaviour obj = new UnityEngine.AudioBehaviour();
                ToLua.Push(L, obj);
                return 1;
            }
            else
            {
                return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: UnityEngine.AudioBehaviour.New");
            }
        }
        catch (Exception e)
        {
            return LuaDLL.toluaL_exception(L, e);
        }
    }

    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    static int op_Equality(IntPtr L)
    {
        try
        {
            ToLua.CheckArgsCount(L, 2);
            UnityEngine.Object arg0 = (UnityEngine.Object)ToLua.ToObject(L, 1);
            UnityEngine.Object arg1 = (UnityEngine.Object)ToLua.ToObject(L, 2);
            bool o = arg0 == arg1;
            LuaDLL.lua_pushboolean(L, o);
            return 1;
        }
        catch (Exception e)
        {
            return LuaDLL.toluaL_exception(L, e);
        }
    }
}


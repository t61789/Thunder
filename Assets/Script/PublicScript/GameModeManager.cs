using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameModeManager
{
    private BaseGameMode curGameMode;

    public BaseGameMode SetupMode(string modeTypeName,Action<BaseGameMode> initAction = null)
    {
        return SetupMode<BaseGameMode>(modeTypeName,initAction);
    }

    public T SetupMode<T>(string modeTypeName, Action<T> initAction=null) where T : BaseGameMode
    {
        Type t = Assembly.GetExecutingAssembly().GetType(modeTypeName);
        return SetupMode(t, initAction);
    }

    public T SetupMode<T>(Action<T> initAction = null) where T : BaseGameMode
    {
        return SetupMode(typeof(T), initAction);
    }

    public T SetupMode<T>(Type modeType, Action<T> initAction=null) where T : BaseGameMode
    {
        if (curGameMode != null)
            RemoveMode();

        T obj = PublicVar.publicVar.AddComponent(modeType) as T;
        initAction?.Invoke(obj);
        curGameMode = obj;
        return obj;
    }

    public void RemoveMode()
    {
        if(curGameMode!=null)
        {
            curGameMode.BeforeUnInstall();
            UnityEngine.Object.Destroy(PublicVar.publicVar.GetComponent<BaseGameMode>());
            curGameMode = null;
        }
    }
}

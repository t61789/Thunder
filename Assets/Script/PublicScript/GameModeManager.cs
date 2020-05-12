using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameModeManager
{
    private BaseGameMode curGameMode;

    public T SetupMode<T>(Action<T> initAction) where T: BaseGameMode
    {
        if (curGameMode != null)
            RemoveMode();

        T obj = PublicVar.publicVar.AddComponent<T>();
        initAction(obj);
        curGameMode = obj;
        return obj;
    }

    public void RemoveMode()
    {
        if(curGameMode!=null)
        {
            curGameMode.UnInstall();
            UnityEngine.Object.Destroy(PublicVar.publicVar.GetComponent<BaseGameMode>() as Component);
            curGameMode = null;
        }
    }
}

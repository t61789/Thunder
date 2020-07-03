using System;
using System.Reflection;
using Thunder.GameMode;

namespace Thunder.PublicScript
{
    public class GameModeManager
    {
        private BaseGameMode curGameMode;

        public BaseGameMode SetupMode(string modeTypeName, string arg, Action<BaseGameMode> init = null)
        {
            return SetupMode<BaseGameMode>(modeTypeName, arg, init);
        }

        public T SetupMode<T>(string modeTypeName, string arg, Action<T> init = null) where T : BaseGameMode
        {
            Type modeType = Assembly.GetExecutingAssembly().GetType(modeTypeName);

            if (curGameMode != null)
                RemoveMode();

            T obj = Sys.Stable.publicVar.AddComponent(modeType) as T;
            init?.Invoke(obj);
            (obj as BaseGameMode).Init(arg);
            curGameMode = obj;
            return obj;
        }

        public void RemoveMode()
        {
            if (curGameMode != null)
            {
                curGameMode.BeforeUnInstall();
                UnityEngine.Object.Destroy(Sys.Stable.publicVar.GetComponent<BaseGameMode>());
                curGameMode = null;
            }
        }
    }
}

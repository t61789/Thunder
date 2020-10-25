using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tool;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Thunder.UI
{
    public class PanelUI:BaseUI, ICanvasRaycastFilter
    {
        public bool Stable = false;

        [HideInInspector] public UnityEvent<PanelUI> OnOpen = new UnityEvent<PanelUI>();
        [HideInInspector] public UnityEvent<PanelUI> OnClose = new UnityEvent<PanelUI>();
        [HideInInspector] public UnityEvent<PanelUI, Action> OnCloseCheck = new UnityEvent<PanelUI, Action>();
        [HideInInspector] public PanelUI Dialog;

        protected virtual void AfterOpen()
        {

        }

        protected virtual void BeforeClose()
        {

        }

        public bool CloseCheck()
        {
            var result = true;
            OnCloseCheck?.Invoke(this, () => result = false);
            if (!result)
                return false;

            OnClose?.Invoke(this);
            return true;
        }

        // [Assetid]:[FunctionName]
        public void CallLuaFunction(string func)
        {
            Assert.IsFalse(string.IsNullOrEmpty(func), $"命令不正确：{func}");
            // ReSharper disable once PossibleNullReferenceException
            var s = func.Split(':');
            Assert.IsTrue(s.Length == 2, $"命令不正确：{func}");
            LuaSys.Ins.ExecuteFile(s[0]);
            LuaSys.Ins.ExecuteCommand(s[1]);
        }

        public void ExecuteLuaCmd(string cmd)
        {
            Assert.IsFalse(string.IsNullOrEmpty(cmd), $"命令不正确：{cmd}");
            LuaSys.Ins.ExecuteCommand(cmd);
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (Dialog == null) return true;
            return !Dialog.gameObject.activeSelf;
        }
    }
}

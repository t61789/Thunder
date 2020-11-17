using System;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public class PanelUI:BaseUI, ICanvasRaycastFilter
    {
        public bool Stable = false;

        [HideInInspector] public event Action OnOpen;
        [HideInInspector] public event Action OnClose;
        /// <summary>
        /// 在请求关闭时触发该事件，若判断不能关闭则执行给定的Action
        /// </summary>
        [HideInInspector] public event Action<Action> OnCloseCheck;
        [HideInInspector] public PanelUI Dialog;

        public virtual void AfterOpen()
        {
            OnOpen?.Invoke();
        }

        public virtual void BeforeClose()
        {
            OnClose?.Invoke();
        }

        public bool CloseCheck()
        {
            var result = true;
            OnCloseCheck?.Invoke( () => result = false);
            return result;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (Dialog == null) return true;
            return !Dialog.gameObject.activeSelf;
        }
    }
}

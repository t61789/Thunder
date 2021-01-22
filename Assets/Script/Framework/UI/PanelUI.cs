using System;
using System.Collections.Generic;
using System.Reflection;
using Thunder.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public class PanelUi : BaseUi, ICanvasRaycastFilter
    {
        public bool Stable = false;
        public string PresetUiActionName;

        [HideInInspector] public event Action OnOpen;
        [HideInInspector] public event Action OnClose;
        /// <summary>
        /// 在请求关闭时触发该事件，若判断不能关闭则执行给定的Action
        /// </summary>
        [HideInInspector] public event Action<Action> OnCloseCheck;
        [HideInInspector] public PanelUi Dialog;

        private string _PresetUiActionName;
        private PresetUiAction _PresetUiActionComponent;
        private bool _TryFindPua;

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
            OnCloseCheck?.Invoke(() => result = false);
            return result;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (Dialog == null) return true;
            return !Dialog.gameObject.activeSelf;
        }

        public void ExecPresetUiAction()
        {
            if (_PresetUiActionComponent == null && !_TryFindPua)
                gameObject.GetComponent<PresetUiAction>();

            if (_PresetUiActionComponent != null)
                _PresetUiActionComponent.Exec();
            else
                _TryFindPua = true;
        }

        private void OnDrawGizmosSelected()
        {
            if (PresetUiActionName != _PresetUiActionName)
            {
                _PresetUiActionName = PresetUiActionName;

                if (_PresetUiActionComponent == null)
                    _PresetUiActionComponent = GetComponent<PresetUiAction>();

                PresetUiAction.AvailablePresetUi.TryGetValue(PresetUiActionName, out Type type);

                if (_PresetUiActionComponent != null && _PresetUiActionComponent.GetType() != type)
                {
                    DestroyImmediate(_PresetUiActionComponent);
                    _PresetUiActionComponent = null;
                }

                if (_PresetUiActionComponent == null && type != null)
                {
                    _PresetUiActionComponent = (PresetUiAction)gameObject.AddComponent(type);
                }
            }
        }
    }
}

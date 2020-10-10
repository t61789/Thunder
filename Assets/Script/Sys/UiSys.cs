using System;
using System.Collections.Generic;
using System.Linq;
using Thunder.Tool.ObjectPool;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Thunder.Sys
{
    public class UISys : IBaseSys
    {
        public static string DefaultUiBundle = Paths.PrefabBundleD + Paths.UIBundle;

        private readonly List<BaseUI> _ActiveUi = new List<BaseUI>();
        private readonly Stack<BaseUI> _CloseStack = new Stack<BaseUI>();
        private readonly List<BaseUI> _HideStableUi = new List<BaseUI>();

        private Transform _UiContainer;
        private Transform _UiRecycleContainer;

        public UISys()
        {
            Ins = this;
            SceneManager.sceneLoaded += (x, y) => Init();
        }

        public static UISys Ins { get; private set; }

        public void OnSceneEnter(string preScene, string curScene)
        {
            Init();
        }

        public void OnSceneExit(string curScene)
        {
        }

        public void OnApplicationExit()
        {
        }

        public UISys Init()
        {
            _UiContainer = GameObject.Find("Canvas").transform.Find("UI");

            _UiRecycleContainer = GameObject.Find("Canvas").transform.Find("Recycle");

            _ActiveUi.Clear();
            _HideStableUi.Clear();
            _CloseStack.Clear();

            var move = new List<Transform>();
            foreach (Transform item in _UiContainer.transform)
            {
                var newui = item.GetComponent<BaseUI>();
                if (item.gameObject.activeSelf)
                {
                    _ActiveUi.Add(newui);
                }
                else
                {
                    _HideStableUi.Add(newui);
                    move.Add(item);
                }
            }

            foreach (var item in move)
                item.SetParent(_UiRecycleContainer);
            return this;
        }

        public BaseUI OpenUI(string uiName, UiInitType act = 0, Action<BaseUI> init = null)
        {
            return OpenUI<BaseUI>(uiName, act, init);
        }

        public BaseUI OpenUI(string uiName, string after, bool dialog = true, UiInitType act = 0,
            Action<BaseUI> init = null)
        {
            return OpenUI<BaseUI>(uiName, after, dialog, act, init);
        }

        //public BaseUI OpenUI(string uiName, BaseUI after, bool dialog = true, UiInitType act = 0, Action<BaseUI> init = null)
        //{
        //    return OpenUI<BaseUI>(uiName,after,dialog, act, init);
        //}

        public T OpenUI<T>(string uiName, UiInitType act = 0, Action<T> init = null) where T : BaseUI
        {
            return OpenUI(new OpenParam<T>(uiName, _UiContainer.childCount, null, act, init));
        }

        public T OpenUI<T>(string uiName, string after, bool dialog = true, UiInitType act = 0, Action<T> init = null)
            where T : BaseUI
        {
            var i = 0;
            for (; i < _ActiveUi.Count; i++)
                if (_ActiveUi[i].UIName == after)
                    break;

            if (i != _ActiveUi.Count)
                return OpenUI(new OpenParam<T>(uiName, i + 1, dialog ? _ActiveUi[i] : null, act, init));
            Debug.LogWarning($"未找到after名为 {after} 的UI");
            return null;
        }

        //public T OpenUI<T>(string uiName, BaseUI after, bool dialog = true, UiInitType act = 0, Action<T> init = null) where T : BaseUI
        //{
        //    return OpenUI(uiName, after.UIName, dialog, act, init);
        //}

        private T OpenUI<T>(OpenParam<T> param) where T : BaseUI
        {
            var plane = _HideStableUi.FirstOrDefault(x => x.UIName == param.UiName);
            if (plane != null)
                _HideStableUi.Remove(plane);

            plane = plane ?? ObjectPool.Ins.Alloc<BaseUI>(null, DefaultUiBundle, param.UiName);

            plane.transform.SetParent(_UiContainer);
            plane.transform.SetSiblingIndex(param.SiblingIndex);

            _ActiveUi.Insert(param.SiblingIndex, plane);

            plane.gameObject.SetActive(true);
            if (param.Dialog != null)
                param.Dialog.Dialog = plane;

            plane.InitRect(param.InitType);

            param.InitAction?.Invoke(plane as T);
            plane.AfterOpen();

            return plane as T;
        }

        public void CloseUI(string uiName, bool force = false)
        {
            var baseUi = _ActiveUi.FirstOrDefault(x => x.UIName == uiName);
            Assert.IsNotNull(baseUi, $"没有名为 {uiName} 的UI");
            CloseUI(baseUi, force);
        }

        private void CloseUI(BaseUI baseUi, bool force = false)
        {
            if (!force && baseUi.Dialog != null) return;

            _CloseStack.Clear();
            _CloseStack.Push(baseUi);

            var back = false;

            while (_CloseStack.Count != 0)
            {
                var curUi = _CloseStack.Peek();

                if (back)
                {
                    _CloseStack.Pop();

                    if (!curUi.BeforeClose()) return;

                    curUi.transform.SetParent(_UiRecycleContainer);
                    _ActiveUi.Remove(curUi);
                    if (curUi.Stable)
                    {
                        _HideStableUi.Add(curUi);
                        curUi.gameObject.SetActive(false);
                    }
                    else
                    {
                        ObjectPool.Ins.Recycle(curUi);
                    }

                    continue;
                }

                if (DialogOpened(curUi.Dialog))
                    _CloseStack.Push(curUi.Dialog);
                else
                    back = true;
            }
        }

        public bool IsUIOpened(BaseUI baseUi)
        {
            return GetUI(baseUi.UIName) != null;
        }

        public bool IsUIOpened(string uiName)
        {
            return GetUI(uiName) != null;
        }

        public void SwitchUI(string planeName)
        {
            if (IsUIOpened(planeName))
                CloseUI(planeName);
            else
                OpenUI(planeName);
        }

        public static bool DialogOpened(BaseUI dialog)
        {
            return dialog != null && dialog.gameObject.activeSelf;
        }

        private BaseUI GetUI(string uiName)
        {
            return _ActiveUi.FirstOrDefault(x => x.UIName == uiName);
        }

        private readonly struct OpenParam<T> where T : BaseUI
        {
            public readonly string UiName;
            public readonly BaseUI Dialog;
            public readonly UiInitType InitType;
            public readonly Action<T> InitAction;
            public readonly int SiblingIndex;

            public OpenParam(string uiName, int siblingIndex, BaseUI dialog, UiInitType initType, Action<T> initAction)
            {
                Assert.IsNotNull(uiName, "UI名不能为null");
                UiName = uiName;
                Dialog = dialog;
                InitType = initType;
                InitAction = initAction;
                SiblingIndex = siblingIndex;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

using Thunder.UI;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Tool
{
    public class UISys : IBaseSys
    {
        public static string DefaultUIBundle = Paths.PrefabBundleD + Paths.UIBundle;

        private readonly List<PanelUI> _ActiveUI = new List<PanelUI>();
        private readonly Stack<PanelUI> _CloseStack = new Stack<PanelUI>();
        private readonly List<PanelUI> _HideStableUI = new List<PanelUI>();

        private Transform _UIContainer;
        private Transform _UIRecycleContainer;

        public UISys()
        {
            Ins = this;
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
            _UIContainer = GameObject.Find("Canvas").transform.Find("UI");

            _UIRecycleContainer = GameObject.Find("Canvas").transform.Find("Recycle");

            _ActiveUI.Clear();
            _HideStableUI.Clear();
            _CloseStack.Clear();

            var move = new List<Transform>();
            foreach (Transform item in _UIContainer.transform)
            {
                var newui = item.GetComponent<PanelUI>();
                if (item.gameObject.activeSelf)
                {
                    _ActiveUI.Add(newui);
                }
                else
                {
                    _HideStableUI.Add(newui);
                    move.Add(item);
                }
            }

            foreach (var item in move)
                item.SetParent(_UIRecycleContainer);
            return this;
        }

        public PanelUI OpenUI(string uiName, UiInitType act = 0)
        {
            return OpenUI<PanelUI>(uiName, act);
        }

        public PanelUI OpenUI(string uiName, string after, bool dialog = true, UiInitType act = 0)
        {
            return OpenUI<PanelUI>(uiName, after, dialog, act);
        }

        public T OpenUI<T>(string uiName, UiInitType act = 0) where T : PanelUI
        {
            return OpenUI<T>(new OpenParam(uiName, _UIContainer.childCount, null, act));
        }

        public T OpenUI<T>(string uiName, string after, bool dialog = true, UiInitType act = 0)
            where T : PanelUI
        {
            var index = _ActiveUI.FindIndex(x => x.EntityName == after);

            if (index != _ActiveUI.Count)
                return OpenUI<T>(new OpenParam(uiName, index + 1, dialog ? _ActiveUI[index] : null, act));
            Debug.LogWarning($"未找到after名为 {after} 的UI");
            return null;
        }

        private T OpenUI<T>(OpenParam param) where T : PanelUI
        {
            var panel = _HideStableUI.FirstOrDefault(x => x.EntityName == param.UiName);
            if (panel != null)
                _HideStableUI.Remove(panel);

            panel = panel ?? ObjectPool.Ins.Alloc<PanelUI>(new AssetId(DefaultUIBundle,param.UiName));

            panel.transform.SetParent(_UIContainer);
            panel.transform.SetSiblingIndex(param.SiblingIndex);

            _ActiveUI.Insert(param.SiblingIndex, panel);

            panel.gameObject.SetActive(true);
            if (param.Dialog != null)
                param.Dialog.Dialog = panel;

            panel.InitRect(param.InitType);

            panel.OnOpen?.Invoke(panel);

            return panel as T;
        }

        public void CloseUI(string uiName, bool force = false)
        {
            var baseUi = _ActiveUI.FirstOrDefault(x => x.EntityName == uiName);
            Assert.IsNotNull(baseUi, $"没有名为 {uiName} 的UI");
            CloseUI(baseUi, force);
        }

        private void CloseUI(PanelUI panelUI, bool force = false)
        {
            if (!force && panelUI.Dialog != null) return;

            _CloseStack.Clear();
            _CloseStack.Push(panelUI);

            var back = false;

            while (_CloseStack.Count != 0)
            {
                var curUi = _CloseStack.Peek();

                if (back)
                {
                    _CloseStack.Pop();

                    if (!curUi.CloseCheck()) return;

                    curUi.transform.SetParent(_UIRecycleContainer);
                    _ActiveUI.Remove(curUi);
                    if (curUi.Stable)
                    {
                        _HideStableUI.Add(curUi);
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

        public bool IsUIOpened(PanelUI panelUI)
        {
            return GetUI(panelUI.EntityName) != null;
        }

        public bool IsUIOpened(string uiName)
        {
            return GetUI(uiName) != null;
        }

        public void SwitchUI(string uiName)
        {
            if (IsUIOpened(uiName))
                CloseUI(uiName);
            else
                OpenUI(uiName);
        }

        public static bool DialogOpened(PanelUI dialog)
        {
            return dialog != null && dialog.gameObject.activeSelf;
        }

        private PanelUI GetUI(string uiName)
        {
            return _ActiveUI.FirstOrDefault(x => x.EntityName == uiName);
        }

        private readonly struct OpenParam
        {
            public readonly string UiName;
            public readonly PanelUI Dialog;
            public readonly UiInitType InitType;
            public readonly int SiblingIndex;

            public OpenParam(string uiName, int siblingIndex, PanelUI dialog, UiInitType initType)
            {
                Assert.IsNotNull(uiName, "UI名不能为null");
                UiName = uiName;
                Dialog = dialog;
                InitType = initType;
                SiblingIndex = siblingIndex;
            }
        }
    }
}
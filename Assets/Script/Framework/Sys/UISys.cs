﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Framework
{
    public class UISys : IBaseSys
    {
        public static readonly string DefaultUIBundle = Paths.PrefabBundle.PCombine(Paths.Normal);

        private static readonly List<PanelUI> _ActiveUI = new List<PanelUI>();
        private static readonly Stack<PanelUI> _CloseStack = new Stack<PanelUI>();
        private static readonly List<PanelUI> _HideStableUI = new List<PanelUI>();

        private static Transform _UIContainer;
        private static Transform _UIRecycleContainer;

        public static void Init()
        {
            _UIContainer = GameObject.Find(Config.UiFramworkBaseObjName).transform.Find("UI");

            _UIRecycleContainer = GameObject.Find(Config.UiFramworkBaseObjName).transform.Find("Recycle");

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
        }

        public static PanelUI OpenUI(string uiName, UiInitType act = 0)
        {
            return OpenUI<PanelUI>(uiName, act);
        }

        public static PanelUI OpenUI(string uiName, string after, bool dialog = true, UiInitType act = 0)
        {
            return OpenUI<PanelUI>(uiName, after, dialog, act);
        }

        public static T OpenUI<T>(string uiName, UiInitType act = 0) where T : PanelUI
        {
            return OpenUI<T>(new OpenParam(uiName, _UIContainer.childCount, null, act));
        }

        public static T OpenUI<T>(string uiName, string after, bool dialog = true, UiInitType act = 0)
            where T : PanelUI
        {
            var index = _ActiveUI.FindIndex(x => x.EntityName == after);

            if (index != _ActiveUI.Count)
                return OpenUI<T>(new OpenParam(uiName, index + 1, dialog ? _ActiveUI[index] : null, act));
            Debug.LogWarning($"未找到after名为 {after} 的UI");
            return null;
        }

        private static T OpenUI<T>(OpenParam param) where T : PanelUI
        {
            var panel = _HideStableUI.FirstOrDefault(x => x.EntityName == param.UiName);
            if (panel != null)
                _HideStableUI.Remove(panel);

            panel = panel ?? ObjectPool.Get<PanelUI>(new AssetId(DefaultUIBundle, param.UiName));

            panel.transform.SetParent(_UIContainer);
            panel.transform.SetSiblingIndex(param.SiblingIndex);

            _ActiveUI.Insert(param.SiblingIndex, panel);

            panel.gameObject.SetActive(true);
            if (param.Dialog != null)
                param.Dialog.Dialog = panel;

            panel.InitRect(param.InitType);

            panel.AfterOpen();

            return panel as T;
        }

        public static void CloseUI(string uiName, bool force = false)
        {
            var baseUi = _ActiveUI.FirstOrDefault(x => x.EntityName == uiName);
            Assert.IsNotNull(baseUi, $"没有名为 {uiName} 的UI");
            CloseUI(baseUi, force);
        }

        private static void CloseUI(PanelUI panelUI, bool force = false)
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
                    curUi.BeforeClose();

                    curUi.transform.SetParent(_UIRecycleContainer);
                    _ActiveUI.Remove(curUi);
                    if (curUi.Stable)
                    {
                        _HideStableUI.Add(curUi);
                        curUi.gameObject.SetActive(false);
                    }
                    else
                    {
                        ObjectPool.Put(curUi);
                    }

                    continue;
                }

                if (DialogOpened(curUi.Dialog))
                    _CloseStack.Push(curUi.Dialog);
                else
                    back = true;
            }
        }

        public static bool IsUIOpened(PanelUI panelUI)
        {
            return GetUI(panelUI.EntityName) != null;
        }

        public static bool IsUIOpened(string uiName)
        {
            return GetUI(uiName) != null;
        }

        public static void SwitchUI(string uiName)
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

        private static PanelUI GetUI(string uiName)
        {
            return _ActiveUI.FirstOrDefault(x => x.EntityName == uiName);
        }

        public void OnSceneEnter(string preScene, string curScene)
        {
            Init();
        }

        public void OnSceneExit(string curScene) { }

        public void OnApplicationExit() { }

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

    public enum DialogResult
    {
        Ok,
        Cancel
    }

    [Flags]
    public enum UiInitType
    {
        /// <summary>
        ///     将UI相对与锚点的位置设为0
        /// </summary>
        PositionMiddleOfAnchor = 1,

        /// <summary>
        ///     将锚点设置为0和1
        /// </summary>
        FillAnchor = PositionMiddleOfAnchor << 1,

        /// <summary>
        ///     将锚点设置为0.5
        /// </summary>
        MiddleAnchor = FillAnchor << 1,

        /// <summary>
        ///     将Offset设置为0
        /// </summary>
        FillSize = MiddleAnchor << 1,

        /// <summary>
        ///     充满父容器并居中
        /// </summary>
        FillParent = FillAnchor | PositionMiddleOfAnchor | FillSize,

        /// <summary>
        ///     在父容器居中
        /// </summary>
        CenterParent = MiddleAnchor | PositionMiddleOfAnchor
    }
}
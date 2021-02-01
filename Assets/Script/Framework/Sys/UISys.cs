using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Framework
{
    public class UiSys : IBaseSys
    {
        public static readonly string DefaultUIBundle = Paths.PrefabBundle.PCombine(Paths.Normal);

        private static readonly List<PanelUi> _ActiveUI = new List<PanelUi>();
        private static readonly Stack<PanelUi> _CloseStack = new Stack<PanelUi>();
        private static readonly List<PanelUi> _HideStableUI = new List<PanelUi>();

        private static Transform _UIContainer;
        private static Transform _UIRecycleContainer;

        public static void Init()
        {
            _UIContainer = GameObject.Find(Config.UiFrameworkBaseObjName).transform.Find("UI");

            _UIRecycleContainer = GameObject.Find(Config.UiFrameworkBaseObjName).transform.Find("Recycle");

            _ActiveUI.Clear();
            _HideStableUI.Clear();
            _CloseStack.Clear();

            var move = new List<Transform>();
            foreach (Transform item in _UIContainer.transform)
            {
                var newui = item.GetComponent<PanelUi>();
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

        public static PanelUi OpenUi(string uiName, UiInitType act = 0)
        {
            return OpenUi<PanelUi>(uiName, act);
        }

        public static PanelUi OpenUi(string uiName, string after, bool dialog = true, UiInitType act = 0)
        {
            return OpenUi<PanelUi>(uiName, after, dialog, act);
        }

        public static T OpenUi<T>(string uiName, UiInitType act = 0) where T : PanelUi
        {
            return OpenUi<T>(new OpenParam(uiName, _UIContainer.childCount, null, act));
        }

        public static T OpenUi<T>(string uiName, string after, bool dialog = true, UiInitType act = 0)
            where T : PanelUi
        {
            var index = _ActiveUI.FindIndex(x => x.EntityName == after);

            if (index != _ActiveUI.Count)
                return OpenUi<T>(new OpenParam(uiName, index + 1, dialog ? _ActiveUI[index] : null, act));
            Debug.LogWarning($"未找到after名为 {after} 的UI");
            return null;
        }

        private static T OpenUi<T>(OpenParam param) where T : PanelUi
        {
            var panel = _HideStableUI.FirstOrDefault(x => x.EntityName == param.UiName);
            if (panel != null)
                _HideStableUI.Remove(panel);

            panel = panel ?? GameObjectPool.Get<PanelUi>(new AssetId(DefaultUIBundle, param.UiName));

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

        public static void CloseUi(string uiName, bool force = false)
        {
            var baseUi = _ActiveUI.FirstOrDefault(x => x.EntityName == uiName);
            Assert.IsNotNull(baseUi, $"没有名为 {uiName} 的UI");
            CloseUi(baseUi, force);
        }

        private static void CloseUi(PanelUi panelUI, bool force = false)
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
                        GameObjectPool.Put(curUi);
                    }

                    continue;
                }

                if (DialogOpened(curUi.Dialog))
                    _CloseStack.Push(curUi.Dialog);
                else
                    back = true;
            }
        }

        public static bool IsUiOpened(PanelUi panelUI)
        {
            return GetUi(panelUI.EntityName) != null;
        }

        public static bool IsUiOpened(string uiName)
        {
            return GetUi(uiName) != null;
        }

        public static void SwitchUi(string uiName)
        {
            if (IsUiOpened(uiName))
                CloseUi(uiName);
            else
                OpenUi(uiName);
        }

        public static bool DialogOpened(PanelUi dialog)
        {
            return dialog != null && dialog.gameObject.activeSelf;
        }

        private static PanelUi GetUi(string uiName)
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
            public readonly PanelUi Dialog;
            public readonly UiInitType InitType;
            public readonly int SiblingIndex;

            public OpenParam(string uiName, int siblingIndex, PanelUi dialog, UiInitType initType)
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
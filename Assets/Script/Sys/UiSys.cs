using System;
using System.Collections.Generic;
using System.Linq;
using Thunder.UI;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Sys
{
    public class UiSys
    {
        private readonly struct OpenParam<T> where T:BaseUi
        {
            public readonly string UiName;
            public readonly BaseUi Dialog;
            public readonly UiInitType InitType;
            public readonly Action<T> InitAction;
            public readonly int SiblingIndex;

            public OpenParam(string uiName,int siblingIndex, BaseUi dialog, UiInitType initType, Action<T> initAction)
            {
                Assert.IsNotNull(uiName,"UI名不能为null");
                UiName = uiName;
                Dialog = dialog;
                InitType = initType;
                InitAction = initAction;
                SiblingIndex = siblingIndex;
            }
        }

        private Transform _UiContainer;
        private Transform _UiRecycleContainer;

        private readonly List<BaseUi> _ActiveUi = new List<BaseUi>();
        private readonly List<BaseUi> _HideStableUi = new List<BaseUi>();
        private readonly Stack<BaseUi> _CloseStack = new Stack<BaseUi>();

        public static string DefaultUiBundle = BundleSys.PrefabBundleD + BundleSys.UIBundle;

        public UiSys()
        {
            Init();
        }

        public void Init()
        {
            _UiContainer = GameObject.Find("Canvas").transform.Find("UI");

            _UiRecycleContainer = GameObject.Find("Canvas").transform.Find("Recycle");

            var move = new List<Transform>();
            foreach (Transform item in _UiContainer.transform)
            {
                var newui = item.GetComponent<BaseUi>();
                if (item.gameObject.activeSelf)
                    _ActiveUi.Add(newui);
                else
                {
                    _HideStableUi.Add(newui);
                    move.Add(item);
                }
            }
            foreach (var item in move)
                item.SetParent(_UiRecycleContainer);
        }

        public BaseUi OpenUi(string uiName, UiInitType act = 0, Action<BaseUi> init = null)
        {
            return OpenUi<BaseUi>(uiName, act, init);
        }

        public BaseUi OpenUi(string uiName, string after, bool dialog = true, UiInitType act = 0, Action<BaseUi> init = null)
        {
            return OpenUi<BaseUi>(uiName, after, dialog, act, init);
        }

        //public BaseUi OpenUi(string uiName, BaseUi after, bool dialog = true, UiInitType act = 0, Action<BaseUi> init = null)
        //{
        //    return OpenUi<BaseUi>(uiName,after,dialog, act, init);
        //}

        public T OpenUi<T>(string uiName, UiInitType act = 0, Action<T> init = null) where T : BaseUi
        {
            return OpenUi(new OpenParam<T>(uiName, _UiContainer.childCount, null, act, init));
        }

        public T OpenUi<T>(string uiName, string after, bool dialog = true, UiInitType act = 0, Action<T> init = null) where T : BaseUi
        {
            int i = 0;
            for (; i < _ActiveUi.Count; i++)
                if (_ActiveUi[i].UiName == after)
                    break;

            if (i != _ActiveUi.Count) return OpenUi(new OpenParam<T>(uiName, i + 1, dialog ? _ActiveUi[i] : null, act, init));
            Debug.LogWarning($"未找到after名为 {after} 的UI");
            return null;
        }

        //public T OpenUi<T>(string uiName, BaseUi after, bool dialog = true, UiInitType act = 0, Action<T> init = null) where T : BaseUi
        //{
        //    return OpenUi(uiName, after.UiName, dialog, act, init);
        //}

        private T OpenUi<T>(OpenParam<T> param) where T:BaseUi
        {
            BaseUi plane = _HideStableUi.FirstOrDefault(x => x.UiName == param.UiName);
            if (plane != null)
                _HideStableUi.Remove(plane);

            plane = plane ?? Stable.ObjectPool.Alloc<BaseUi>(null, DefaultUiBundle, param.UiName);

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

        public void CloseUi(string uiName,bool force=false)
        {
            BaseUi baseUi = _ActiveUi.FirstOrDefault(x => x.UiName == uiName);
            Assert.IsNotNull(baseUi, $"没有名为 {uiName} 的UI");
            CloseUi(baseUi, force);
        }

        private void CloseUi(BaseUi baseUi, bool force = false)
        {
            if (!force && baseUi.Dialog != null) return;

            _CloseStack.Clear();
            _CloseStack.Push(baseUi);

            bool back = false;

            while (_CloseStack.Count != 0)
            {
                BaseUi curUi = _CloseStack.Peek();

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
                        Stable.ObjectPool.Recycle(curUi);

                    continue;
                }

                if (DialogOpened(curUi.Dialog))
                    _CloseStack.Push(curUi.Dialog);
                else
                    back = true;
            }
        }

        public bool IsUiOpened(BaseUi baseUi)
        {
            return GetUi(baseUi.UiName) != null;
        }

        public bool IsUiOpened(string uiName)
        {
            return GetUi(uiName) != null;
        }

        public void SwitchUi(string planeName)
        {
            if (IsUiOpened(planeName))
                CloseUi(planeName);
            else
                OpenUi(planeName);
        }

        public static bool DialogOpened(BaseUi dialog)
        {
            return dialog != null && dialog.gameObject.activeSelf;
        }

        private BaseUi GetUi(string uiName)
        {
            return _ActiveUi.FirstOrDefault(x=>x.UiName==uiName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.PublicScript;
using Assets.Script.UI;
using Assets.Script.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Script.System
{
    public class UiSys : MonoBehaviour
    {
        private Transform _UiContainer;
        private Transform _UiRecycleContainer;

        private class UiUnit
        {
            //public int siblingIndex;
            public readonly BaseUi UiObj;

            public UiUnit(int siblingIndex, BaseUi uiObj)
            {
                //this.siblingIndex = siblingIndex;
                this.UiObj = uiObj;
            }
        }

        private readonly List<UiUnit> _ActiveUi = new List<UiUnit>();
        private readonly List<UiUnit> _HideStableUi = new List<UiUnit>();
        private readonly Stack<UiUnit> _CloseStack = new Stack<UiUnit>();

        public static string DefaultUiBundle = BundleSys.PrefabBundleD+BundleSys.UIBundle;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            _UiContainer = GameObject.Find("Canvas").transform.Find("UI");

            _UiRecycleContainer = GameObject.Find("Canvas").transform.Find("Recycle");

            int count = 0;
            List<Transform> move = new List<Transform>();
            foreach (Transform item in _UiContainer.transform)
            {
                UiUnit newui = new UiUnit(count, item.GetComponent<BaseUi>());
                if (item.gameObject.activeSelf)
                    _ActiveUi.Add(newui);
                else
                {
                    _HideStableUi.Add(newui);
                    move.Add(item);
                }
                count++;
            }
            foreach (var item in move)
                item.SetParent(_UiRecycleContainer);
        }

        public BaseUi OpenUi(string uiName, UiInitAction act = 0, Action<BaseUi> init = null)
        {
            return OpenUi<BaseUi>(uiName, act, init);
        }

        public T OpenUi<T>(string uiName, UiInitAction act = 0, Action<T> init = null) where T : BaseUi
        {
            return OpenUi(uiName, true, act, init);
        }

        public T OpenUi<T>(string uiName, string after, bool dialog = false, UiInitAction act = 0, Action<T> init = null) where T : BaseUi
        {
            UiUnit find = _ActiveUi.FirstOrDefault(x => x.UiObj.UiName == after);

            if (find == null)
            {
                Debug.LogError("No such ui named " + after + " which you want to insert " + uiName + " after");
                return null;
            }

            return OpenUi(uiName, _ActiveUi.IndexOf(find) + 1, dialog ? find.UiObj : null, act, init);
        }

        public T OpenUi<T>(string uiName, BaseUi after, bool dialog = false, UiInitAction act = 0, Action<T> init = null) where T : BaseUi
        {
            UiUnit find = _ActiveUi.FirstOrDefault(x => x.UiObj == after);
            if (find == null)
            {
                Debug.LogError("No such ui named [" + after.UiName + "] which you want to insert [" + uiName + "] after");
                return null;
            }

            return OpenUi(uiName, _ActiveUi.IndexOf(find) + 1, dialog ? after : null, act, init);
        }

        public T OpenUi<T>(string uiName, bool last, UiInitAction act = 0, Action<T> init = null) where T : BaseUi
        {

            if (last)
                return OpenUi(uiName, _UiContainer.transform.childCount, null, act, init);
            else
                return OpenUi(uiName, 0, null, act, init);
        }

        public T OpenUi<T>(string uiName, int siblingIndex, BaseUi dialog = null, UiInitAction act = 0, Action<T> init = null) where T : BaseUi
        {
            if (siblingIndex < 0 || siblingIndex > _UiContainer.childCount)
            {
                Debug.LogError("Index out of range");
                return null;
            }

            UiUnit unit = _HideStableUi.FirstOrDefault(x => x.UiObj.UiName == uiName);
            if (unit != null)
                _HideStableUi.Remove(unit);

            BaseUi newPlane = unit?.UiObj;
            if (newPlane == null)
                newPlane = System.objectPool.Alloc<BaseUi>(null, DefaultUiBundle, uiName);

            newPlane.transform.SetParent(_UiContainer);
            newPlane.transform.SetSiblingIndex(siblingIndex);
            unit = unit??new UiUnit(siblingIndex, newPlane);

            _ActiveUi.Insert(siblingIndex, unit);

            newPlane.gameObject.SetActive(true);
            if (dialog != null)
                dialog.Dialog = newPlane;

            newPlane.InitRect(act);

            T result = newPlane as T;
            init?.Invoke(result);
            newPlane.AfterOpen();

            return result;
        }

        public void CloseUi(string uiName)
        {
            CloseUi(_ActiveUi.FirstOrDefault(x => x.UiObj.UiName == uiName));
        }

        public void CloseUi(BaseUi ui)
        {
            CloseUi(_ActiveUi.FirstOrDefault(x => x.UiObj == ui));
        }

        private void CloseUi(UiUnit unit)
        {
            Assert.IsNotNull(unit,$"没有名为 {unit.UiObj.name} 的UI");

            _CloseStack.Clear();
            _CloseStack.Push(unit);

            bool back = false;

            while (_CloseStack.Count != 0)
            {
                UiUnit curUnit = _CloseStack.Peek();

                if (back)
                {
                    _CloseStack.Pop();

                    if (!curUnit.UiObj.BeforeClose()) return;

                    curUnit.UiObj.transform.SetParent(_UiRecycleContainer);
                    _ActiveUi.Remove(curUnit);
                    if (curUnit.UiObj.Stable)
                    {
                        _HideStableUi.Add(curUnit);
                        curUnit.UiObj.gameObject.SetActive(false);
                    }
                    else
                        System.objectPool.Recycle(curUnit.UiObj);

                    continue;
                }

                if (DialogOpened(curUnit.UiObj.Dialog))
                {
                    _CloseStack.Push(_ActiveUi.FirstOrDefault(x => x.UiObj == curUnit.UiObj.Dialog));
                }
                else
                    back = true;
            }
        }

        public bool IsUiOpened(string planeName)
        {
            return _ActiveUi.Find(x => x.UiObj.UiName == planeName) != null;
        }

        public void SwitchUi(string planeName)
        {
            if (IsUiOpened(planeName))
                CloseUi(planeName);
            else
                OpenUi<BaseUi>(planeName);
        }

        public static bool DialogOpened(BaseUi dialog)
        {
            return dialog != null && dialog.gameObject.activeSelf;
        }
    }
}

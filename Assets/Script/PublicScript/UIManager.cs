using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private Transform uiContainer;
    private Transform uiRecycleContainer;

    private class UIUnit
    {
        public int siblingIndex;
        public BaseUI uiObj;

        public UIUnit(int siblingIndex, BaseUI uiObj)
        {
            this.siblingIndex = siblingIndex;
            this.uiObj = uiObj;
        }
    }

    private readonly List<UIUnit> activeUi = new List<UIUnit>();
    private readonly List<UIUnit> hideStableUi = new List<UIUnit>();
    private readonly Stack<UIUnit> closeStack = new Stack<UIUnit>();

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        uiContainer = GameObject.Find("Canvas").transform.Find("UI");
        uiRecycleContainer = GameObject.Find("Canvas").transform.Find("Recycle");

        int count = 0;
        List<Transform> move = new List<Transform>();
        foreach (Transform item in uiContainer.transform)
        {
            UIUnit newui = new UIUnit(count, item.GetComponent<BaseUI>());
            if (item.gameObject.activeSelf)
                activeUi.Add(newui);
            else
            {
                hideStableUi.Add(newui);
                move.Add(item);
            }
            count++;
        }
        foreach (var item in move)
            item.SetParent(uiRecycleContainer);
    }

    public BaseUI OpenUI(string uiName,UIInitAction act=0, Action<BaseUI> init = null)
    {
        return OpenUI<BaseUI>(uiName, act, init);
    }

    public T OpenUI<T>(string uiName, UIInitAction act = 0, Action<T> init = null) where T : BaseUI
    {
        return OpenUI(uiName, true, act, init);
    }

    public T OpenUI<T>(string uiName, string after, bool dialog = false, UIInitAction act = 0, Action<T> init = null) where T : BaseUI
    {
        UIUnit find = activeUi.Where(x => x.uiObj.UIName == after).FirstOrDefault();

        if (find == null)
        {
            Debug.LogError("No such ui named " + after + " which you want to insert " + uiName + " after");
            return null;
        }

        return OpenUI(uiName, activeUi.IndexOf(find) + 1, dialog?find.uiObj:null, act, init);
    }

    public T OpenUI<T>(string uiName, BaseUI after, bool dialog = false, UIInitAction act = 0, Action<T> init = null) where T : BaseUI
    {
        UIUnit find = activeUi.Where(x => x.uiObj == after).FirstOrDefault();
        if (find == null)
        {
            Debug.LogError("No such ui which you want to insert " + uiName + " after");
            return null;
        }

        return OpenUI(uiName, activeUi.IndexOf(find) + 1, dialog?after:null, act, init);
    }

    public T OpenUI<T>(string uiName, bool last, UIInitAction act = 0, Action<T> init = null) where T : BaseUI
    {
        if (last)
            return OpenUI(uiName, uiContainer.transform.childCount, null, act, init);
        else
            return OpenUI(uiName, 0, null, act, init);
    }

    public T OpenUI<T>(string uiName, int siblingIndex, BaseUI dialog = null,UIInitAction act=0, Action<T> init = null) where T : BaseUI
    {
        if (siblingIndex < 0 || siblingIndex > uiContainer.childCount)
        {
            Debug.LogError("Index out of range");
            return null;
        }

        UIUnit unit = hideStableUi.Where(x =>  x.uiObj.UIName == uiName).FirstOrDefault();
        if (unit != null)
            hideStableUi.Remove(unit);

        BaseUI newPlane = unit?.uiObj;
        if (newPlane == null)
            newPlane = PublicVar.objectPool.Alloc<BaseUI>(BundleManager.UIBundle, uiName);

        newPlane.transform.SetParent(uiContainer);
        newPlane.transform.SetSiblingIndex(siblingIndex);
        if (unit == null) unit = new UIUnit(siblingIndex, newPlane);

        activeUi.Insert(siblingIndex, unit);

        newPlane.gameObject.SetActive(true);
        if (dialog!=null)
            dialog.dialog = newPlane;

        newPlane.InitRect(act);

        T result = newPlane as T;
        init?.Invoke(result);
        newPlane.AfterOpen();

        return result;
    }

    public void CloseUI(string uiName)
    {
        CloseUI(activeUi.Where(x => x.uiObj.UIName == uiName).FirstOrDefault());
    }

    public void CloseUI(BaseUI ui)
    {
        CloseUI(activeUi.Where(x => x.uiObj == ui).FirstOrDefault());
    }

    private void CloseUI(UIUnit unit)
    {
        if (unit == null)
        {
            Debug.LogError("No such ui named " + unit.uiObj.UIName);
            return;
        }

        closeStack.Clear();
        closeStack.Push(unit);

        bool back = false;

        while (closeStack.Count != 0)
        {
            UIUnit curUnit = closeStack.Peek();

            if (back)
            {
                closeStack.Pop();

                if (!curUnit.uiObj.BeforeClose()) return;

                curUnit.uiObj.transform.SetParent(uiRecycleContainer);
                activeUi.Remove(curUnit);
                if (curUnit.uiObj.Stable)
                {
                    hideStableUi.Add(curUnit);
                    curUnit.uiObj.gameObject.SetActive(false);
                }
                else
                    PublicVar.objectPool.Recycle(curUnit.uiObj);

                continue;
            }

            if (DialogOpened(curUnit.uiObj.dialog))
            {
                closeStack.Push(activeUi.Where(x => x.uiObj == curUnit.uiObj.dialog).FirstOrDefault());
                continue;
            }
            else
                back = true;
        }
    }

    public bool IsUIOpened(string planeName)
    {
        return activeUi.Find(x => x.uiObj.UIName == planeName) != null;
    }

    public void SwitchUI(string planeName)
    {
        if (IsUIOpened(planeName))
            CloseUI(planeName);
        else
            OpenUI<BaseUI>(planeName);
    }

    public static bool DialogOpened(BaseUI dialog)
    {
        if (dialog == null) return false;
        return dialog.gameObject.activeSelf;
    }
}

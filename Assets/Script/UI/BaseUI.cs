﻿using System;
using System.Collections;
using Tool.ObjectPool;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class BaseUI : MonoBehaviour, IObjectPool, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, ICanvasRaycastFilter
{
    public string uiId;
    public bool Stable = false;

    [HideInInspector]
    public RectTransform rectTrans;

    public delegate void PointerDel(BaseUI baseUI, PointerEventData eventData);
    public event PointerDel PointerDown;
    public event PointerDel PointerEnter;
    public event PointerDel PointerClick;
    public event PointerDel PointerExit;
    public event PointerDel PointerUp;

    public delegate void AfterOpenDel(BaseUI baseUI);
    public event AfterOpenDel OnAfterOpen;
    public delegate void BeforeCloseDel(BaseUI baseUI);
    public event BeforeCloseDel OnBeforeClose;
    public delegate void CloseCheck(BaseUI baseUI,ref bool result);
    public event CloseCheck OnCloseCheck;

    public BaseUI dialog;

    public virtual void Awake()
    {
        if (uiId == null)
            uiId = name;
        rectTrans = GetComponent<RectTransform>();
    }

    public virtual void AfterOpen()
    {
        OnAfterOpen?.Invoke(this);
    }

    public virtual bool BeforeClose()
    {
        bool result = true;
        OnCloseCheck?.Invoke(this,ref result);
        if (!result)
            return false;

        OnBeforeClose?.Invoke(this);
        return true;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void ObjectPoolDestroy()
    {
        Destroy(gameObject);
    }

    public void ObjectPoolRecycle()
    {

    }

    public virtual void ObjectPoolReset(Hashtable arg)
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PointerClick?.Invoke(this, eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDown?.Invoke(this, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter?.Invoke(this, eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExit?.Invoke(this, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerUp?.Invoke(this, eventData);
    }

    public void ObjectPoolReset()
    {

    }

    public void SetAnchor(Vector2 anchorMax, Vector2 anchorMin)
    {
        rectTrans.anchorMin = anchorMin;
        rectTrans.anchorMax = anchorMax;
    }

    public void SetOffset(Vector2 offsetMax, Vector2 offsetMin)
    {
        rectTrans.offsetMax = offsetMax;
        rectTrans.offsetMin = offsetMin;
    }

    public void SetAnchoredPosition(Vector2 pos)
    {
        rectTrans.position = pos;
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return dialog == null;
    }

    public void Close()
    {
        PublicVar.uiManager.CloseUI(this);
    }
}

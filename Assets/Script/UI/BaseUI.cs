using System.Collections;
using Assets.Script.PublicScript;
using Assets.Script.System;
using Assets.Script.Tool.ObjectPool;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Script.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class BaseUi : MonoBehaviour, IObjectPool, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, ICanvasRaycastFilter, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public bool Stable = false;

        public string UiName
        {
            set => _UiName = value;

            get => string.IsNullOrEmpty(_UiName) ? name : _UiName;
        }

        [SerializeField]
        private string _UiName;

        [HideInInspector]
        public RectTransform RectTrans;

        public delegate void PointerDel(BaseUi baseUi, PointerEventData eventData);
        public event PointerDel PointerDown;
        public event PointerDel PointerEnter;
        public event PointerDel PointerClick;
        public event PointerDel PointerExit;
        public event PointerDel PointerUp;
        public event PointerDel DragStart;
        public event PointerDel DragEnd;
        public event PointerDel Dragging;

        public delegate void AfterOpenDel(BaseUi baseUi);
        public event AfterOpenDel OnAfterOpen;
        public delegate void BeforeCloseDel(BaseUi baseUi);
        public event BeforeCloseDel OnBeforeClose;
        public delegate void CloseCheck(BaseUi baseUi, ref bool result);
        public event CloseCheck OnCloseCheck;

        [HideInInspector]
        public BaseUi Dialog;

        protected virtual void Awake()
        {
            UiName = UiName ?? name;
            RectTrans = transform as RectTransform;
        }

        [ContextMenu("fff")]
        private void Shit()
        {
            Debug.Log(123);
        }

        public virtual void AfterOpen()
        {
            OnAfterOpen?.Invoke(this);
        }

        public virtual bool BeforeClose()
        {
            bool result = true;
            OnCloseCheck?.Invoke(this, ref result);
            if (!result)
                return false;

            OnBeforeClose?.Invoke(this);
            return true;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void AfterOpDestroy()
        {
            Destroy(gameObject);
        }

        public AssetId AssetId { get; set; }

        public void BeforeOpRecycle()
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

        public void OnBeginDrag(PointerEventData eventData)
        {
            DragStart?.Invoke(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragEnd?.Invoke(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Dragging?.Invoke(this, eventData);
        }

        public void BeforeOpReset()
        {

        }

        public void SetAnchor(Vector2 anchorMax, Vector2 anchorMin)
        {
            RectTrans.anchorMin = anchorMin;
            RectTrans.anchorMax = anchorMax;
        }

        public void SetOffset(Vector2 offsetMax, Vector2 offsetMin)
        {
            RectTrans.offsetMax = offsetMax;
            RectTrans.offsetMin = offsetMin;
        }

        public void SetAnchoredPosition(Vector2 pos)
        {
            RectTrans.position = pos;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (Dialog == null) return true;
            return !Dialog.gameObject.activeSelf;
        }

        public void Close()
        {
            PublicVar.UiSys.CloseUi(this);
        }

        public void InitRect(UIInitAction action)
        {
            if ((action & UIInitAction.MiddleAnchor) != 0)
            {
                RectTrans.anchorMin = RectTrans.anchorMax = Vector2.one / 2;
            }

            if ((action & UIInitAction.FillAnchor) != 0)
            {
                RectTrans.anchorMax = Vector2.one;
                RectTrans.anchorMin = Vector2.zero;
            }

            if ((action & UIInitAction.FillSize) != 0)
            {
                RectTrans.offsetMin = RectTrans.offsetMax = Vector2.zero;
            }

            if ((action & UIInitAction.PositionMiddleOfAnchor) != 0)
            {
                RectTrans.anchoredPosition = Vector2.zero;
            }
        }
    }
}

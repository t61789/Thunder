using System.Collections;
using Tool.ObjectPool;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Script.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class BaseUI : MonoBehaviour, IObjectPool, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, ICanvasRaycastFilter, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public bool Stable = false;

        public string UIName
        {
            set
            {
                _UIName = value;
            }

            get
            {
                if (_UIName == null || _UIName == "")
                    return name;
                else
                    return _UIName;
            }
        }

        [SerializeField]
        private string _UIName;

        [HideInInspector]
        public RectTransform rectTrans;

        public delegate void PointerDel(BaseUI baseUI, PointerEventData eventData);
        public event PointerDel PointerDown;
        public event PointerDel PointerEnter;
        public event PointerDel PointerClick;
        public event PointerDel PointerExit;
        public event PointerDel PointerUp;
        public event PointerDel DragStart;
        public event PointerDel DragEnd;
        public event PointerDel Dragging;

        public delegate void AfterOpenDel(BaseUI baseUI);
        public event AfterOpenDel OnAfterOpen;
        public delegate void BeforeCloseDel(BaseUI baseUI);
        public event BeforeCloseDel OnBeforeClose;
        public delegate void CloseCheck(BaseUI baseUI, ref bool result);
        public event CloseCheck OnCloseCheck;

        [HideInInspector]
        public BaseUI dialog;

        protected virtual void Awake()
        {
            if (UIName == null)
                UIName = name;
            rectTrans = transform as RectTransform;
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
            if (dialog == null) return true;
            return !dialog.gameObject.activeSelf;
        }

        public void Close()
        {
            PublicVar.uiManager.CloseUI(this);
        }

        public void InitRect(UIInitAction action)
        {
            if ((action & UIInitAction.MiddleAnchor) != 0)
            {
                rectTrans.anchorMin = rectTrans.anchorMax = Vector2.one / 2;
            }

            if ((action & UIInitAction.FillAnchor) != 0)
            {
                rectTrans.anchorMax = Vector2.one;
                rectTrans.anchorMin = Vector2.zero;
            }

            if ((action & UIInitAction.FillSize) != 0)
            {
                rectTrans.offsetMin = rectTrans.offsetMax = Vector2.zero;
            }

            if ((action & UIInitAction.PositionMiddleOfAnchor) != 0)
            {
                rectTrans.anchoredPosition = Vector2.zero;
            }
        }

    }
}

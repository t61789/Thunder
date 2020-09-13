using System.Collections;
using Thunder.Sys;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Thunder.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class BaseUI : MonoBehaviour, IObjectPool, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, ICanvasRaycastFilter, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public bool Stable = false;

        public string UIName
        {
            set => _UIName = value;

            get => string.IsNullOrEmpty(_UIName) ? name : _UIName;
        }

        [SerializeField]
        private string _UIName;

        [HideInInspector]
        public RectTransform RectTrans;

        public delegate void PointerDel(BaseUI baseUi, PointerEventData eventData);
        public event PointerDel PointerDown;
        public event PointerDel PointerEnter;
        public event PointerDel PointerClick;
        public event PointerDel PointerExit;
        public event PointerDel PointerUp;
        public event PointerDel DragStart;
        public event PointerDel DragEnd;
        public event PointerDel Dragging;

        public delegate void AfterOpenDel(BaseUI baseUi);
        public event AfterOpenDel OnAfterOpen;
        public delegate void BeforeCloseDel(BaseUI baseUi);
        public event BeforeCloseDel OnBeforeClose;
        public delegate void CloseCheck(BaseUI baseUi, ref bool result);
        public event CloseCheck OnCloseCheck;

        [HideInInspector]
        public BaseUI Dialog;

        protected virtual void Awake()
        {
            UIName = UIName ?? name;
            RectTrans = transform as RectTransform;
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
            Sys.Stable.UI.CloseUI(UIName);
        }

        public void InitRect(UiInitType action)
        {
            if ((action & UiInitType.MiddleAnchor) != 0)
            {
                RectTrans.anchorMin = RectTrans.anchorMax = Vector2.one / 2;
            }

            if ((action & UiInitType.FillAnchor) != 0)
            {
                RectTrans.anchorMax = Vector2.one;
                RectTrans.anchorMin = Vector2.zero;
            }

            if ((action & UiInitType.FillSize) != 0)
            {
                RectTrans.offsetMin = RectTrans.offsetMax = Vector2.zero;
            }

            if ((action & UiInitType.PositionMiddleOfAnchor) != 0)
            {
                RectTrans.anchoredPosition = Vector2.zero;
            }
        }

        // [Assetid]:[FunctionName]
        public void CallLuaFunction(string func)
        {
            Assert.IsFalse(string.IsNullOrEmpty(func), $"命令不正确：{func}");
            // ReSharper disable once PossibleNullReferenceException
            var s = func.Split(':');
            Assert.IsTrue(s.Length == 2, $"命令不正确：{func}");
            Sys.Stable.Lua.ExecuteFile(s[0]);
            Sys.Stable.Lua.ExecuteCommand(s[1]);
        }

        public void ExecuteLuaCmd(string cmd)
        {
            Assert.IsFalse(string.IsNullOrEmpty(cmd), $"命令不正确：{cmd}");
            // ReSharper disable once PossibleNullReferenceException
            var s = cmd.Split(':');
            Assert.IsTrue(s.Length == 2, $"命令不正确：{cmd}");
            Sys.Stable.Lua.ExecuteFile(s[0]);
            Sys.Stable.Lua.ExecuteCommand(s[1]);
        }
    }
}

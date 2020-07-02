using Tool.ObjectPool;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Script.UI
{
    public class DragMoveButton : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        private RectTransform parentRectTrans;

        private Vector3 recordParentPos;
        private Vector3 recordMousePos;

        private void Awake()
        {
            parentRectTrans = transform.parent as RectTransform;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            recordParentPos = parentRectTrans.position;
            recordMousePos = Input.mousePosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 delta = Input.mousePosition - recordMousePos;

            parentRectTrans.position = recordParentPos + delta;
        }
    }
}

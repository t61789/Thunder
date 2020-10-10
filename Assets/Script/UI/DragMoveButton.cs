using UnityEngine;
using UnityEngine.EventSystems;

namespace Thunder.UI
{
    public class DragMoveButton : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        private RectTransform parentRectTrans;
        private Vector3 recordMousePos;

        private Vector3 recordParentPos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            recordParentPos = parentRectTrans.position;
            recordMousePos = Input.mousePosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var delta = Input.mousePosition - recordMousePos;

            parentRectTrans.position = recordParentPos + delta;
        }

        private void Awake()
        {
            parentRectTrans = transform.parent as RectTransform;
        }
    }
}
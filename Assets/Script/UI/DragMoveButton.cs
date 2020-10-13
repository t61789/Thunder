using UnityEngine;
using UnityEngine.EventSystems;

namespace Thunder.UI
{
    public class DragMoveButton : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        private RectTransform _ParentRectTrans;
        private Vector3 _RecordMousePos;
        private Vector3 _RecordParentPos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _RecordParentPos = _ParentRectTrans.position;
            _RecordMousePos = Input.mousePosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var delta = Input.mousePosition - _RecordMousePos;

            _ParentRectTrans.position = _RecordParentPos + delta;
        }

        private void Awake()
        {
            _ParentRectTrans = transform.parent as RectTransform;
        }
    }
}
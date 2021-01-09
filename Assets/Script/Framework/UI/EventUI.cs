using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Framework
{
    public class EventUi:BaseUi, IPointerClickHandler, IPointerUpHandler,
        IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public PointerEvent PointerClickE { get; } = new PointerEvent();
        public PointerEvent PointerUpE { get; } = new PointerEvent();
        public PointerEvent PointerDownE { get; } = new PointerEvent();
        public PointerEvent DragE { get; } = new PointerEvent();
        public PointerEvent BeginDragE { get; } = new PointerEvent();
        public PointerEvent EndDragE { get; } = new PointerEvent();

        public void OnPointerClick(PointerEventData eventData)
        {
            PointerClickE?.Invoke(this, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerDownE?.Invoke(this, eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PointerUpE?.Invoke(this, eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            BeginDragE?.Invoke(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            EndDragE?.Invoke(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            DragE?.Invoke(this, eventData);
        }
    }
}

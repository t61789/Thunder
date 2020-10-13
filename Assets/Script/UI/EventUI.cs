using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Thunder.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Thunder.UI
{
    public class EventUI:BaseUI, IPointerClickHandler, IPointerUpHandler,
        IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public UnityEvent<EventUI,PointerEventData> PointerClickE { get; } = 
            new UnityEvent<EventUI, PointerEventData>();
        public UnityEvent<EventUI,PointerEventData> PointerUpE { get; } = 
            new UnityEvent<EventUI, PointerEventData>();
        public UnityEvent<EventUI, PointerEventData> PointerDownE { get; } =
            new UnityEvent<EventUI, PointerEventData>();
        public UnityEvent<EventUI, PointerEventData> DragE { get; } =
            new UnityEvent<EventUI, PointerEventData>();
        public UnityEvent<EventUI, PointerEventData> BeginDragE { get; } =
            new UnityEvent<EventUI, PointerEventData>();
        public UnityEvent<EventUI, PointerEventData> EndDragE { get; } =
            new UnityEvent<EventUI, PointerEventData>();

        public void OnPointerClick(PointerEventData eventData)
        {
            PointerClickE?.Invoke(this,eventData);
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

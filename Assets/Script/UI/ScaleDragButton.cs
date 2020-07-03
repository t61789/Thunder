using UnityEngine;
using UnityEngine.EventSystems;

namespace Thunder.UI
{
    public class ScaleDragButton : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public Vector2 maxSize = Vector2.positiveInfinity;
        public Vector2 minSize = Vector2.negativeInfinity;

        private RectTransform parentRect;
        private Vector3 recordMousePos;
        private Vector3 recordParentSize;
        private Vector3 recordParentPos;

        private void Awake()
        {
            parentRect = transform.parent as RectTransform;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            recordMousePos = Input.mousePosition;
            recordParentSize = parentRect.rect.size;
            recordParentPos = parentRect.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 delta = Input.mousePosition - recordMousePos;

            float temp = recordParentSize.x + delta.x;
            if (temp > maxSize.x)
            {
                temp = maxSize.x;
                delta.x = maxSize.x - recordParentSize.x;
            }
            else if (temp < minSize.x)
            {
                temp = minSize.x;
                delta.x = minSize.x - recordParentSize.x;
            }
            parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, temp);

            temp = recordParentSize.y - delta.y;
            if (temp > maxSize.y)
            {
                temp = maxSize.y;
                delta.y = -maxSize.y + recordParentSize.y;
            }
            else if (temp < minSize.y)
            {
                temp = minSize.y;
                delta.y = -minSize.y + recordParentSize.y;
            }
            parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, temp);

            parentRect.position = recordParentPos + delta / 2;
        }
    }
}

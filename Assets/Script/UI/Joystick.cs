using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class Joystick : PanelUI
    {
        public bool StablePanel;
        public int Index;
        public float StayTime = 0.7f;
        public float HideTime = 1f;
        public float CapRadius = 59.21f;
        public float Radius = 203.6f;
        public float ShowTime = 0.1f;

        private static Value[] _Values = new Value[0];
        private float _AlphaCount;
        private EventUI _Cap;
        private float _CurAlpha;
        private bool _Dragging;
        private float _FadeCount;
        private Material _ImageMaterial;
        private BaseUI _Panel;
        private Image _PanelImage;

        protected void Start()
        {
            var valueList = new List<Value>(_Values);
            for (var i = valueList.Count; i <= Index; i++)
                valueList.Add(new Value());
            _Values = valueList.ToArray();

            _Cap = transform.Find("Cap").GetComponent<EventUI>();
            _Panel = transform.Find("Panel").GetComponent<BaseUI>();
            _PanelImage = _Panel.GetComponent<Image>();

            _Cap.DragE.AddListener(OnCapDrag);
            _Cap.EndDragE.AddListener(OnCapEndDrag);
            _Cap.BeginDragE.AddListener(OnCapBeginDrag);
            _Cap.PointerClickE.AddListener(OnCapPointerClick);
            _Cap.PointerDownE.AddListener(OnCapPointerDown);
            _Cap.PointerUpE.AddListener(OnCapPointerUp);

            _ImageMaterial = new Material(_PanelImage.material);
            _PanelImage.material = _ImageMaterial;
        }

        private void Update()
        {
            if (!StablePanel)
            {
                if (!_Dragging && _CurAlpha > 0 && Time.time - _FadeCount >= StayTime)
                {
                    _CurAlpha = _AlphaCount - (Time.time - _FadeCount - StayTime) / HideTime;

                    if (_CurAlpha <= 0)
                        _Values[Index].Val = Vector3.zero;
                    _ImageMaterial.SetFloat("_GlobalAlpha", _CurAlpha);
                }
                else if (_Dragging && _CurAlpha < 1)
                {
                    _CurAlpha = _AlphaCount + (Time.time - _FadeCount) / ShowTime;
                    _ImageMaterial.SetFloat("_GlobalAlpha", _CurAlpha);
                }
            }
        }

        private void LateUpdate()
        {
            _Values[Index].Click = false;
            _Values[Index].DoubleClick = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, CapRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }

        public void OnCapBeginDrag(EventUI ui, PointerEventData eventData)
        {
            var pos = eventData.position;
            if ((pos - (Vector2) _Cap.RectTrans.position).magnitude < CapRadius)
            {
                _Dragging = true;
                if (!StablePanel)
                {
                    _FadeCount = Time.time;
                    _AlphaCount = _CurAlpha;

                    if (_CurAlpha <= 0)
                        _Panel.RectTrans.position = _Cap.RectTrans.position;
                }
            }
        }

        public void OnCapEndDrag(EventUI ui, PointerEventData eventData)
        {
            if (_Dragging)
            {
                _Dragging = false;
                _Values[Index].Val = Vector3.zero;

                if (StablePanel)
                {
                    _Cap.RectTrans.position = _Panel.RectTrans.position;
                }
                else
                {
                    _FadeCount = Time.time;
                    _AlphaCount = _CurAlpha;
                }
            }
        }

        public void OnCapDrag( EventUI ui, PointerEventData eventData)
        {
            if (_Dragging)
            {
                var temp = (Vector3) eventData.position - _Panel.RectTrans.position;
                if (StablePanel)
                {
                    if (temp.magnitude > Radius)
                        temp = temp.normalized * Radius;
                    _Cap.RectTrans.position = _Panel.RectTrans.position + temp;
                }
                else
                {
                    _Cap.RectTrans.position = eventData.position;
                    if (temp.magnitude > Radius)
                    {
                        temp = temp.normalized * Radius;
                        _Panel.RectTrans.position = -temp.normalized * Radius + _Cap.RectTrans.position;
                    }
                }

                _Values[Index].Val = temp.normalized * (temp.magnitude / Radius);
            }
        }

        public void OnCapPointerClick(EventUI ui, PointerEventData eventData)
        {
            if (_Dragging) return;

            if (eventData.clickCount == 1)
                _Values[Index].Click = true;
            else
                _Values[Index].DoubleClick = true;
        }

        public void OnCapPointerDown(EventUI ui, PointerEventData eventData)
        {
            _Values[Index].Holding = true;
        }

        public void OnCapPointerUp(EventUI ui, PointerEventData eventData)
        {
            _Values[Index].Holding = false;
        }

        public static Value GetValue(int index)
        {
            if (index < 0 || index >= _Values.Length)
                return new Value();
            return _Values[index];
        }

        public struct Value
        {
            public Vector3 Val;
            public bool Click;
            public bool DoubleClick;
            public bool Holding;
        }
    }
}
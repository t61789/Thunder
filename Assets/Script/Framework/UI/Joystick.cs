using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework
{
    public class Joystick : PanelUi
    {
        public string CtrKey;
        public bool StablePanel;
        public float StayTime = 0.7f;
        public float HideTime = 1f;
        public float CapRadius = 59.21f;
        public float Radius = 203.6f;
        public float ShowTime = 0.1f;

        private float _AlphaCount;
        private EventUi _Cap;
        private float _CurAlpha;
        private bool _Dragging;
        private float _FadeCount;
        private Material _ImageMaterial;
        private BaseUi _Panel;
        private Image _PanelImage;
        private ControlInfo _CurControlInfo;

        protected void Start()
        {
            _Cap = transform.Find("Cap").GetComponent<EventUi>();
            _Panel = transform.Find("Panel").GetComponent<BaseUi>();
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
                        _CurControlInfo.Axis = Vector3.zero;
                    _ImageMaterial.SetFloat("_GlobalAlpha", _CurAlpha);
                }
                else if (_Dragging && _CurAlpha < 1)
                {
                    _CurAlpha = _AlphaCount + (Time.time - _FadeCount) / ShowTime;
                    _ImageMaterial.SetFloat("_GlobalAlpha", _CurAlpha);
                }
            }

            ControlSys.InjectValue(CtrKey,_CurControlInfo);
            _CurControlInfo.Down = false;
            _CurControlInfo.Up = false;
            _CurControlInfo.Click = false;
            _CurControlInfo.DoubleClick = false;
        }

        public void OnCapBeginDrag(EventUi ui, PointerEventData eventData)
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

        public void OnCapEndDrag(EventUi ui, PointerEventData eventData)
        {
            if (_Dragging)
            {
                _Dragging = false;
                _CurControlInfo.Axis = Vector3.zero;

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

        public void OnCapDrag( EventUi ui, PointerEventData eventData)
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

                _CurControlInfo.Axis = temp.normalized * (temp.magnitude / Radius);
            }
        }

        public void OnCapPointerClick(EventUi ui, PointerEventData eventData)
        {
            if (_Dragging) return;

            if (eventData.clickCount == 1)
                _CurControlInfo.Click = true;
            else
                _CurControlInfo.DoubleClick = true;
        }

        public void OnCapPointerDown(EventUi ui, PointerEventData eventData)
        {
            _CurControlInfo.Down = true;
            _CurControlInfo.Stay = true;
        }

        public void OnCapPointerUp(EventUi ui, PointerEventData eventData)
        {
            _CurControlInfo.Up = true;
            _CurControlInfo.Stay = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, CapRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}
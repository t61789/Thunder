using Thunder.Sys;
using Thunder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Thunder.Entity
{
    public class PackageCell:BaseUI,IBeginDragHandler,IPointerClickHandler,IPointerUpHandler,ICanvasRaycastFilter
    {
        public bool ShowCount=true;

        private RawImage _RawImage;
        private TextMeshProUGUI _CountText;
        private RectTransform _FloatContainer;
        private bool _Floatting;
        private Vector3 _FollowOffset;

        public int PackageIndex { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _RawImage = GetComponent<RawImage>();
            _CountText = RectTrans.Find("CountText").GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (!_Floatting) return;
            RectTrans.position = Input.mousePosition + _FollowOffset;
        }

        public virtual void Init(int packageIndex,ItemGroup itemGroup,RectTransform floatUiContainer)
        {
            _FloatContainer = floatUiContainer;
            _RawImage.texture = BundleSys.Ins.GetAsset<Texture>(ItemSys.Ins[itemGroup.Id].PackageCellTexturePath);
            _CountText.text = ShowCount ? itemGroup.Count.ToString():string.Empty;
            PackageIndex = packageIndex;
            PackageIndex = -1;
            _FloatContainer = null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_Floatting) return;
            TakeUp();
            _Floatting = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_Floatting) return;
            TakeUp();
            _Floatting = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_Floatting) return;
            PutDown();
            _Floatting = false;
        }

        private void TakeUp()
        {
            _FollowOffset = RectTrans.position - Input.mousePosition;
            RectTrans.SetParent(_FloatContainer);
        }

        private void PutDown()
        {
            // if !on
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return !_Floatting;
        }
    }
}

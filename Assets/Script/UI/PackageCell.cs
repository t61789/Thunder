using Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Thunder
{
    public class PackageCell:BaseUI,IBeginDragHandler,IPointerClickHandler,IPointerUpHandler,ICanvasRaycastFilter
    {// todo 点击菜单栏
        public bool ShowCount=true;

        private Package _Package;
        private RawImage _RawImage;
        private int _PackageIndex;
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

        public virtual void Init(Package package,int packageIndex,RectTransform floatUiContainer)
        {
            _FloatContainer = floatUiContainer;
            var group = package.GetCell(packageIndex);
            _PackageIndex = packageIndex;
            _Package = package;
            _RawImage.texture = BundleSys.GetAsset<Texture>(ItemSys.Ins[group.Id].PackageCellTexturePath);
            _CountText.text = ShowCount ? group.Count.ToString():string.Empty;
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

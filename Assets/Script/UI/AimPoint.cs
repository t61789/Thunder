using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Thunder.UI
{
    public class AimPoint : BaseUi
    {
        public float HitScaleMag = 2;
        public float HitTime = 0.3f;
        public float HitStayTime = 0.8f;
        public float HitFadeTime = 1;
        public float FireInterval = 0.3f;

        private RectTransform _HitTrans;
        private Image _HitImage;
        private Material _HitMat;

        private Vector2 _ScaledSize;
        private Vector2 _UnScaledSize;
        private bool _Hitting;
        private float _HitStayTimeCount;
        private float _FireIntervalCount;

        private const string ProperyName = "_TransparentMag";

        protected override void Awake()
        {
            base.Awake();

            _HitTrans = RectTrans.Find("Hit") as RectTransform;
            _HitImage = _HitTrans.GetComponent<Image>();
            _HitMat = _HitImage.material;

            _UnScaledSize = _HitTrans.sizeDelta;
            _ScaledSize = _UnScaledSize * HitScaleMag;
            _HitTrans.sizeDelta = _ScaledSize;

            _HitMat.SetFloat(ProperyName, 0);
        }

        public void Hit()
        {
            _HitStayTimeCount = HitStayTime;

            if (!_Hitting)
            {
                _Hitting = true;
                _HitTrans.DOKill();
                _HitTrans.DOSizeDelta(_UnScaledSize, HitTime);
            }

            _HitMat.SetFloat(ProperyName, 0);

            _HitMat.DOKill();
            _HitMat.DOFloat(1, ProperyName, HitTime);
        }

        private void FixedUpdate()
        {
            _FireIntervalCount -= _FireIntervalCount > 0 ? Time.deltaTime : 0;
            if (_FireIntervalCount <= 0 && Input.GetKey(KeyCode.Mouse0))
            {
                _FireIntervalCount = FireInterval;
                Hit();
            }

            if (_HitStayTimeCount <= 0) return;
            _HitStayTimeCount -= Time.fixedDeltaTime;
            if (_HitStayTimeCount > 0) return;
            _Hitting = false;
            _HitTrans.DOSizeDelta(_ScaledSize, HitFadeTime);
            _HitMat.DOFloat(0, ProperyName, HitFadeTime);
        }
    }
}

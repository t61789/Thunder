using Thunder.Entity;
using Tool;
using UnityEngine;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class AimPoint : BaseUI
    {
        public static AimPoint Ins;

        public float HitFadeTime = 1;
        public float HitStayTime = 0.8f;
        public Sprite AimTex;
        public Sprite HitTex;
        public Shader HitShader;
        public float BaseHitSize;
        public RectTransform HitTrans;
        public RectTransform AimTrans;

        private readonly GameObject[] _Images = new GameObject[8];
        private Material _AimMat;
        private Material _HitMat;
        private SimpleCounterQueue _HitCounterQueue;

        private const string PROPERY_NAME = "_TransparentMag";

        protected override void Awake()
        {
            base.Awake();
            Ins = this;
            
            _HitMat = new Material(HitShader);
            _AimMat = new Material(_HitMat);

            _HitMat.SetFloat(PROPERY_NAME, 0);

            ResetAll();

            _HitCounterQueue = new SimpleCounterQueue(this,new SimpleCounter(0),
                new []{HitStayTime,HitFadeTime});
        }

        private void FixedUpdate()
        {
            if (_HitCounterQueue.CurStage == 1)
                _HitMat.SetFloat(PROPERY_NAME, 1 - _HitCounterQueue.Counter.Interpolant);

            SetAimValue(Player.Ins.WeaponBelt.CurrentWeapon.OverHeatFactor);
        }

        public void Hit()
        {
            _HitMat.SetFloat(PROPERY_NAME, 1);
            _HitCounterQueue.Play();
        }

        private void SetAimValue(float value)
        {
            var size = BaseHitSize * value;
            AimTrans.sizeDelta = new Vector2(size, size);
        }

        private void ResetAll()
        {
            foreach (var img in _Images)
                Destroy(img);
            var anchor = Vector3.up;
            var rot = Quaternion.AngleAxis(90, Vector3.forward);
            var curRot = Quaternion.AngleAxis(0, Vector3.forward);
            for (var i = 0; i < 8; i++)
            {
                var go = new GameObject();
                var img = go.AddComponent<Image>();
                img.sprite = i < 4 ? AimTex : HitTex;
                img.material = i < 4 ? _AimMat : _HitMat;
                img.SetNativeSize();
                var rt = go.transform as RectTransform;
                rt.SetParent(i < 4 ? AimTrans : HitTrans);
                rt.anchorMin = rt.anchorMax = (anchor + Vector3.one) / 2;
                rt.anchoredPosition = Vector2.zero;
                rt.rotation = curRot;
                _Images[i] = go;

                if (i == 3)
                {
                    anchor = Vector3.one;
                    curRot = Quaternion.AngleAxis(45, Vector3.forward);
                }
                else
                {
                    curRot *= rot;
                    anchor = rot * anchor;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Tools.ScreenMiddle, new Vector3(BaseHitSize, BaseHitSize, 0));
        }
    }
}
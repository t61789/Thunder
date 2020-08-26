using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Thunder.Tool;
using Thunder.Tool.BuffData;
using Thunder.Utility;

namespace Thunder.UI
{
    public class AimPoint : BaseUi
    {
        public static AimPoint Instance;

        [SerializeField]
        private Vector2 _AimSize;
        public Vector2 HitSize;
        public Sprite AimTex;
        public Sprite HitTex;
        public RectTransform AimTrans;
        public RectTransform HitTrans;
        public float AimValue
        {
            get=>_AimValue;
            set => SetAimValue(value);
        }
        private float _AimValue;
        public float AimTime = 0.02f;
        public float HitTime = 0.3f;
        public float HitStayTime = 0.8f;
        public float HitFadeTime = 1;
        public Shader HitShader;

        [HideInInspector]
        public BuffData AimSizeScale = 1;
        public Vector2 AimSize => _AimSize * AimSizeScale.CurData;

        private Material _HitMat;
        private Material _AimMat;
        private bool _Hitting;
        private float _HitStayTimeCount;

        private readonly GameObject[] _Images = new GameObject[8];

        private const string ProperyName = "_TransparentMag";

        protected override void Awake()
        {
            base.Awake();

            _HitMat = new Material(HitShader);
            _AimMat = new Material(_HitMat);

            _HitMat.SetFloat(ProperyName, 0);

            ResetAll();

            Instance = this;
        }

        public void Hit()
        {
            _HitStayTimeCount = HitStayTime;

            if (!_Hitting)
            {
                _Hitting = true;
                HitTrans.DOKill();
                HitTrans.DOSizeDelta(new Vector2(HitSize.x, HitSize.x), HitTime);
            }

            _HitMat.SetFloat(ProperyName, 0);

            _HitMat.DOKill();
            _HitMat.DOFloat(1, ProperyName, HitTime);
        }

        public void SetAimValue(float value)
        {
            _AimValue = value;
            float size = Mathf.Lerp(AimSize.x,AimSize.y,value);
            AimTrans.sizeDelta = new Vector2(size,size);
        }

        private void FixedUpdate()
        {
            if (_HitStayTimeCount <= 0) return;
            _HitStayTimeCount -= Time.fixedDeltaTime;
            if (_HitStayTimeCount > 0) return;
            _Hitting = false;
            HitTrans.DOSizeDelta(new Vector2(HitSize.y, HitSize.y), HitFadeTime);
            _HitMat.DOFloat(0, ProperyName, HitFadeTime);
        }

        private void ResetAll()
        {
            foreach (var img in _Images)
                Destroy(img);
            Vector3 anchor = Vector3.up;
            Quaternion rot = Quaternion.AngleAxis(90, Vector3.forward);
            Quaternion curRot = Quaternion.AngleAxis(0, Vector3.forward);
            for (int i = 0; i < 8; i++)
            {
                GameObject go = new GameObject();
                Image img = go.AddComponent<Image>();
                img.sprite = i < 4 ? AimTex : HitTex;
                img.material = i < 4 ? _AimMat : _HitMat;
                img.SetNativeSize();
                RectTransform rt = go.transform as RectTransform;
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

            AimValue = 0;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Tools.screenMiddle, new Vector3(AimSize.x, AimSize.x, 0));
            Gizmos.DrawWireCube(Tools.screenMiddle, new Vector3(AimSize.y, AimSize.y, 0));
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Tools.screenMiddle, new Vector3(HitSize.x, HitSize.x, 0));
            Gizmos.DrawWireCube(Tools.screenMiddle, new Vector3(HitSize.y, HitSize.y, 0));
        }
    }
}

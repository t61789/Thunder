using DG.Tweening;
using Thunder.Game.FlyingSaucer;

using Thunder.Utility;
using TMPro;
using Tool;
using UnityEngine;

namespace Thunder.UI
{
    public class FlyingSaucerScoreBoard : BaseUI
    {
        private TextMeshProUGUI _BatterText;
        private RectTransform _BatterTextRectTrans;
        private Color _HitJumpBaseColor;

        private Vector2 _HitJumpBaseSize;
        private Sequence _HitJumpTween;
        private TextMeshProUGUI _ScoreText;
        public Color HitJumpColor;
        public Vector2 HitJumpSizeScale;
        public Vector2 HitJumpTime;

        protected override void Awake()
        {
            base.Awake();

            _BatterText = RectTrans.Find("BatterText").GetComponent<TextMeshProUGUI>();
            _BatterTextRectTrans = _BatterText.rectTransform;
            _ScoreText = RectTrans.Find("ScoreText").GetComponent<TextMeshProUGUI>();

            _HitJumpBaseColor = _BatterText.color;
            _HitJumpBaseSize = _BatterTextRectTrans.rect.Size();

            _HitJumpTween = DOTween.Sequence();
            _HitJumpTween.Append(_BatterTextRectTrans.DOFixedSize(_HitJumpBaseSize * HitJumpSizeScale, HitJumpTime.x));
            _HitJumpTween.Append(_BatterTextRectTrans.DOFixedSize(_HitJumpBaseSize, HitJumpTime.y));
            _HitJumpTween.SetAutoKill(false);
            _HitJumpTween.Pause();

            UpdateData(0, 0, 0);
        }

        private void Start()
        {
            FlyingSaucerGame.OnDataChanged += UpdateData;
            PublicEvents.FlyingSaucerHit.AddListener(SaucerHit);
        }

        private void SaucerHit()
        {
            _HitJumpTween.Restart();
        }

        private void UpdateData(float score, int batter, int hits)
        {
            _ScoreText.text = $"Score: {score}";
            _BatterText.text = batter.ToString();
        }

        private void FixedUpdate()
        {
            _BatterText.color =
                Tools.Lerp(HitJumpColor, _HitJumpBaseColor, FlyingSaucerGame.Instance.BatterFadeCounter.Interpolant);
        }
    }
}
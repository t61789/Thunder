using DG.Tweening;
using System.Linq;
using Thunder.Tool;
using Thunder.Utility;
using Thunder.Utility.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class ScoreBoard : BaseUI
    {
        public float ScorePerHit;
        public float BatterFadeTime;
        public int MaxEffectiveBatter;
        public int MaxScoreMagnification;
        public Vector2 HitJumpTime;
        public Vector2 HitJumpSizeScale;
        public Color HitJumpColor;
        public float TurnTime;

        private Counter _TurnCounter;
        private float _CurScore;
        private int _CurHit;
        private Counter _BatterFadeCounter;
        private int _Batter;
        private Vector2 _HitJumpBaseSize;
        private Color _HitJumpBaseColor;
        private TextMeshProUGUI _BatterText;
        private RectTransform _BatterTextRectTrans;
        private TextMeshProUGUI _ScoreText;
        private Sequence _HitJumpTween;

        protected override void Awake()
        {
            base.Awake();
            FlyingSaucerHit.Event.AddListener(SaucerHit);
            _BatterText = RectTrans.Find("BatterText").GetComponent<TextMeshProUGUI>();
            _BatterTextRectTrans = _BatterText.rectTransform;
            _ScoreText = RectTrans.Find("ScoreText").GetComponent<TextMeshProUGUI>();

            _HitJumpBaseColor = _BatterText.color;
            _HitJumpBaseSize = _BatterTextRectTrans.rect.Size();

            _BatterFadeCounter = new Counter(BatterFadeTime,false).OnComplete(() =>
            {
                _Batter = 0;
                UpdateData();
            }).StartCount(this);

            _TurnCounter = new Counter(TurnTime).OnComplete(() =>
                Debug.Log("回合结束")).StartCount(this);

            _HitJumpTween = DOTween.Sequence();
            _HitJumpTween.Append(_BatterTextRectTrans.DOFixedSize(_HitJumpBaseSize * HitJumpSizeScale, HitJumpTime.x));
            _HitJumpTween.Append(_BatterTextRectTrans.DOFixedSize(_HitJumpBaseSize, HitJumpTime.y));
            _HitJumpTween.SetAutoKill(false);
            _HitJumpTween.Pause();

            UpdateData();
        }

        public void StartTurn()
        {
            _Batter = 0;
            _CurHit = 0;
            _CurScore = 0;
        }

        private void SaucerHit()
        {
            float magnification = Tools.Lerp(1, MaxScoreMagnification,
                Tools.InLerp(0, MaxEffectiveBatter, _Batter));
            _CurScore += ScorePerHit * magnification;
            _Batter++;
            _CurHit++;
            _BatterFadeCounter.Recount();
            _HitJumpTween.Restart();

            UpdateData();
        }

        private void UpdateData()
        {
            _ScoreText.text = $"Score: {_CurScore}";
            _BatterText.text = _Batter.ToString();
        }

        private void FixedUpdate()
        {
            _BatterText.color = 
                Tools.Lerp(HitJumpColor, _HitJumpBaseColor, _BatterFadeCounter.Interpolant);
        }
    }
}

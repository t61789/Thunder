using TMPro;
using UnityEngine;

namespace Assets.Script.UI
{
    public class LogPanel : BaseUI
    {
        [HideInInspector]
        public TextMeshProUGUI textMesh;

        public bool ResizeWithText;
        public Vector2 Interval;

        protected override void Awake()
        {
            base.Awake();
            textMesh = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        }

        private void Resize()
        {
            rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textMesh.rectTransform.rect.width + Interval.x);
            rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textMesh.rectTransform.rect.height + Interval.y);
        }

        public string GetText()
        {
            return textMesh.text;
        }

        public void SetText(string text)
        {
            textMesh.SetText(text);
            Resize();
        }
    }
}

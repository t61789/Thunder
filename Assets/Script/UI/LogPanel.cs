using TMPro;
using UnityEngine;

namespace Assets.Script.UI
{
    public class LogPanel : BaseUi
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
            RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textMesh.rectTransform.rect.width + Interval.x);
            RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textMesh.rectTransform.rect.height + Interval.y);
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

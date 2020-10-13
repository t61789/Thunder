using Thunder.Sys;
using Thunder.Utility;
using TMPro;
using UnityEngine;

namespace Thunder.UI
{
    public class ConfirmDialog : BaseUI
    {
        public DialogResult DialogResult;

        private string _TempText;
        public TextMeshProUGUI TextMesh;

        protected override void Awake()
        {
            base.Awake();
            TextMesh = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _TempText = TextMesh.text;
        }

        public void Init(string text)
        {
            TextMesh.SetText(text);
        }

        public void OK()
        {
            DialogResult = DialogResult.Ok;
            UISys.Ins.CloseUI(EntityName);
        }

        public void Cancel()
        {
            DialogResult = DialogResult.Cancel;
            UISys.Ins.CloseUI(EntityName);
        }

        public void Update()
        {
            if (TextMesh.text != _TempText)
            {
                _TempText = TextMesh.text;
                RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TextMesh.rectTransform.rect.width);
            }
        }
    }
}
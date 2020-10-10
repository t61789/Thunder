using Thunder.Sys;
using Thunder.Utility;
using TMPro;
using UnityEngine;

namespace Thunder.UI
{
    public class ConfirmDialog : BaseUI
    {
        public DialogResult dialogResult;

        private string tempText;
        public TextMeshProUGUI textMesh;

        protected override void Awake()
        {
            base.Awake();
            textMesh = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            tempText = textMesh.text;
        }

        public void Init(string text)
        {
            textMesh.SetText(text);
        }

        public void OK()
        {
            dialogResult = DialogResult.Ok;
            UISys.Ins.CloseUI(UIName);
        }

        public void Cancel()
        {
            dialogResult = DialogResult.Cancel;
            UISys.Ins.CloseUI(UIName);
        }

        public void Update()
        {
            if (textMesh.text != tempText)
            {
                tempText = textMesh.text;
                RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textMesh.rectTransform.rect.width);
            }
        }
    }
}
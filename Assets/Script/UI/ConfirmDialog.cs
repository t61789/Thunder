﻿using Thunder.Utility;
using TMPro;

namespace Thunder.UI
{
    public class ConfirmDialog : BaseUI
    {
        public TextMeshProUGUI textMesh;

        public DialogResult dialogResult;

        private string tempText;

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
            Sys.UISys.Ins.CloseUI(UIName);
        }

        public void Cancel()
        {
            dialogResult = DialogResult.Cancel;
            Sys.UISys.Ins.CloseUI(UIName);
        }

        public void Update()
        {
            if (textMesh.text != tempText)
            {
                tempText = textMesh.text;
                RectTrans.SetSizeWithCurrentAnchors(UnityEngine.RectTransform.Axis.Horizontal, textMesh.rectTransform.rect.width);
            }
        }
    }
}

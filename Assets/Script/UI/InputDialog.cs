﻿using System.Collections;
using Framework;

using UnityEngine.UI;

namespace Thunder.UI
{
    public class InputDialog : PanelUi
    {
        public DialogResult dialogResult;

        private InputField inputField;
        public string Text;

        public void Init(string text)
        {
            inputField.text = text;
        }

        protected override void Awake()
        {
            base.Awake();
            inputField = transform.Find("InputField").GetComponent<InputField>();
        }

        public void TextChange()
        {
            Text = inputField.text;
        }

        public void InputEndOK()
        {
            dialogResult = DialogResult.Ok;
            UiSys.CloseUi(EntityName);
        }

        public void InputEndCancel()
        {
            dialogResult = DialogResult.Cancel;
            UiSys.CloseUi(EntityName);
        }

        public override void OpReset()
        {
            inputField.text = null;
        }
    }
}
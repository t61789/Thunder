﻿using System.Collections;
using UnityEngine.UI;

namespace Assets.Script.UI
{
    public class InputDialog : BaseUi
    {
        public string Text;
        public DialogResult dialogResult;

        private InputField inputField;

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
            dialogResult = DialogResult.OK;
            PublicVar.uiManager.CloseUi(this);
        }

        public void InputEndCancel()
        {
            dialogResult = DialogResult.Cancel;
            PublicVar.uiManager.CloseUi(this);
        }

        public override void ObjectPoolReset(Hashtable arg)
        {
            base.ObjectPoolReset(arg);
            inputField.text = null;
        }
    }
}

using System.Collections;
using Thunder.Sys;
using Thunder.Utility;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class InputDialog : BaseUI
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
            UISys.Ins.CloseUI(UIName);
        }

        public void InputEndCancel()
        {
            dialogResult = DialogResult.Cancel;
            UISys.Ins.CloseUI(UIName);
        }

        public override void ObjectPoolReset(Hashtable arg)
        {
            base.ObjectPoolReset(arg);
            inputField.text = null;
        }
    }
}
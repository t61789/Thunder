using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class InputDialog:BaseUI
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
        dialogResult =  DialogResult.OK;
        PublicVar.uiManager.CloseUI(this);
    }

    public void InputEndCancel()
    {
        dialogResult = DialogResult.Cancel;
        PublicVar.uiManager.CloseUI(this);
    }

    public override void ObjectPoolReset(Hashtable arg)
    {
        base.ObjectPoolReset(arg);
        inputField.text = null;
    }
}

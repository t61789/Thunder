using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class BaseButton : BaseUI
    {
        public void Init(string text, Action onClick = null)
        {
            RectTrans.Find("Text").GetComponent<Text>().text = text;
            if (onClick != null)
                GetComponent<Button>().onClick.AddListener(new UnityAction(onClick));
        }
    }
}
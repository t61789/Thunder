using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Script.UI
{
    public class BaseButton : BaseUI
    {
        public void Init(string text, Action onClick = null)
        {
            rectTrans.Find("Text").GetComponent<Text>().text = text;
            if (onClick != null)
                GetComponent<Button>().onClick.AddListener(new UnityAction(onClick));
        }
    }
}

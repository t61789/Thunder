using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.UI;

public class BaseButton:BaseUI
{
    public void Init(string text,Action onClick=null)
    {
        rectTrans.Find("Text").GetComponent<Text>().text = text;
        if(onClick!=null)
            GetComponent<Button>().onClick.AddListener(new UnityAction(onClick));
    }
}

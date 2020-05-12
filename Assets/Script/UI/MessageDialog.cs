using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

public class MessageDialog:BaseUI
{
    private Text text;

    public override void Awake()
    {
        base.Awake();
        text = transform.Find("Text").GetComponent<Text>();
    }
    public void Init(string message)
    {
        text.text = message;
    }
}

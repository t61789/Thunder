using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FairyGUI;
using Thunder.UI;

namespace Thunder
{
    public class GameStartAlertPanel:FairyPanel
    {
        public string HeadTitleName = "headTitle";
        public string TimeCountName = "timeCount";

        private GTextField _HeadTitle;
        private GTextField _TimeCount;

        public static GameStartAlertPanel Ins { private set; get; }

        protected override void Awake()
        {
            base.Awake();

            Ins = this;
            _HeadTitle = UIPanel.ui.GetChild(HeadTitleName).asTextField;
            _TimeCount = UIPanel.ui.GetChild(TimeCountName).asTextField;
        }

        public void SetHeadTitle(string text)
        {
            _HeadTitle.text = text;
        }

        public void SetTimeCount(string text)
        {
            _TimeCount.text = text;
        }
    }
}

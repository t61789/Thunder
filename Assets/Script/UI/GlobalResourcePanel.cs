using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FairyGUI;
using UnityEngine;

namespace Thunder.UI
{
    public class GlobalResourcePanel:FairyPanel
    {
        public string PowerNumTextFieldName = "powerNum";
        public string PsionicNumTextFieldName = "psionicNum";

        private GTextField _PowerTextField;
        private GTextField _PsionicTextField;
        
        private void Start()
        {
            var ui = UIPanel.ui;
            _PowerTextField = ui.GetChild(PowerNumTextFieldName).asTextField;
            _PsionicTextField = ui.GetChild(PsionicNumTextFieldName).asTextField;

            GlobalResource.Ins.Power.OnChanged.AddListener(UpdatePower);
            GlobalResource.Ins.Psionic.OnChanged.AddListener(UpdatePsionic);
        }

        private void OnDestroy()
        {
            GlobalResource.Ins.Power.OnChanged.RemoveListener(UpdatePower);
            GlobalResource.Ins.Psionic.OnChanged.RemoveListener(UpdatePsionic);
        }

        private void UpdatePower(float num)
        {
            _PowerTextField.text = num.ToString(CultureInfo.InvariantCulture);
        }

        private void UpdatePsionic(float num)
        {
            _PsionicTextField.text = num.ToString(CultureInfo.InvariantCulture);
        }
    }
}

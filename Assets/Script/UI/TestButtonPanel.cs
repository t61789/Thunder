using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.UI;

namespace Thunder
{
    public class TestButtonPanel : FairyPanel
    {
        protected override void Awake()
        {
            base.Awake();

            var ui = UIPanel.ui;
            ui.GetChild("n6").asButton.onClick.Add(() => OpenPanel("packagePanel"));
            ui.GetChild("n7").asButton.onClick.Add(() => ClosePanel("packagePanel"));
            ui.GetChild("n8").asButton.onClick.Add(() => BuildingCenter.Ins.EndingBuildingMode());
        }
    }
}

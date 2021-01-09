using System;
using System.Collections.Generic;
using Framework;
using UnityEngine;

namespace Thunder.UI
{
    public abstract class PresetUiAction:MonoBehaviour
    {
        public static readonly Dictionary<string, Type> AvaliablePresetUi = new Dictionary<string, Type>
        {
            {nameof(OpenUi),typeof(OpenUi)},
            {nameof(CloseUi),typeof(CloseUi)},
            {nameof(LogMsg),typeof(LogMsg)}
        };

        public abstract void Exec();
    }

    public class OpenUi : PresetUiAction
    {
        public string UiName;

        public override void Exec()
        {
            UiSys.OpenUi(UiName);
        }
    }

    public class CloseUi : PresetUiAction
    {
        public string UiName;

        public override void Exec()
        {
            UiSys.CloseUi(UiName);
        }
    }

    public class LogMsg : PresetUiAction
    {
        public string Text;

        public override void Exec()
        {
            Debug.Log(Text);
        }
    }
}

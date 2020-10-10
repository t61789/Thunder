using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity
{
    public class Controller : LuaInterface
    {
        [DontInject] [SerializeField] private string _Camp;

        [DontInject] [SerializeField] private string _ControllerName;

        public bool UserControl;

        public string ControllerName
        {
            set => _ControllerName = value;

            get => string.IsNullOrEmpty(_ControllerName) ? name : _ControllerName;
        }

        public string Camp
        {
            set => _Camp = value;

            get => string.IsNullOrEmpty(_Camp) ? GlobalSettings.DefaultCamp : _Camp;
        }
    }
}
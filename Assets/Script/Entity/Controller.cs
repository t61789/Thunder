using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Thunder.Sys;
using LuaInterface;
using Thunder.Tool;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;


namespace Thunder.Entity
{
    public class Controller : LuaScriptInterface
    {
        public string ControllerName
        {
            set => _ControllerName = value;

            get => string.IsNullOrEmpty(_ControllerName) ? name : _ControllerName;
        }

        [DontInject]
        [SerializeField]
        private string _ControllerName;

        public string Camp
        {
            set => _Camp = value;

            get => string.IsNullOrEmpty(_Camp) ? GlobalSettings.DefaultCamp : _Camp;
        }

        [DontInject]
        [SerializeField]
        private string _Camp;

        public bool UserControl;
    }
}

using UnityEngine;

namespace Thunder.Character
{
    public class Controller : MonoBehaviour
    {
        public string ControllerName
        {
            set => _ControllerName = value;

            get
            {
                if (string.IsNullOrEmpty(_ControllerName))
                    return name;
                else
                    return _ControllerName;
            }
        }

        // 一个InputId对应了一系列输入方式和输入名之间的映射方式
        public string InputId
        {
            set => _InputId = value;

            get
            {
                if (string.IsNullOrEmpty(_InputId))
                    return name;
                else
                    return _InputId;
            }
        }

        [SerializeField]
        private string _InputId;
        [SerializeField]
        private string _ControllerName;
        [HideInInspector]
        public readonly Tool.DisposableDictionary ControlKeys = new Tool.DisposableDictionary();
    }
}

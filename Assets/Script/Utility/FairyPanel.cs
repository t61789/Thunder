using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FairyGUI;
using Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Thunder.UI
{
    public class FairyPanel:MonoBehaviour
    {
        public bool ShowOnSceneLoaded;

        public UIPanel UIPanel { private set; get; }
        public string UiName
        {
            get => _UiName.IsNullOrEmpty() ? name : _UiName;
            set => _UiName = value;
        }
        [SerializeField]private string _UiName;

        private static readonly Dictionary<string, FairyPanel> _PanelDic 
            = new Dictionary<string, FairyPanel>();

        static FairyPanel()
        {
            SceneManager.sceneUnloaded += Clear;
        }

        protected virtual void Awake()
        {
            _PanelDic.Add(UiName,this);
            UIPanel = GetComponent<UIPanel>();
            if(!ShowOnSceneLoaded)gameObject.SetActive(false);
        }

        protected virtual void OnOpen() { }

        protected virtual void OnClose() { }

        public static FairyPanel OpenPanel(string uiName)
        {
            var panel = FindPanel(uiName);
            panel.gameObject.SetActive(true);
            panel.OnOpen();
            return panel;
        }

        public static void ClosePanel(string uiName)
        {
            var panel = FindPanel(uiName);
            panel.gameObject.SetActive(false);
            panel.OnClose();
        }

        private static FairyPanel FindPanel(string uiName)
        {
            return _PanelDic.TryGetAndException(uiName, $"未找到名为 {uiName} 的fairyUiPanel");
        }

        private static void Clear(Scene scene)
        {
            _PanelDic.Clear();
        }
    }
}

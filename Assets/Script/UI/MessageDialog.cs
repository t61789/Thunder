using UnityEngine.UI;

namespace Thunder.UI
{
    public class MessageDialog : BaseUI
    {
        private Text text;

        protected override void Awake()
        {
            base.Awake();
            text = transform.Find("Text").GetComponent<Text>();
        }
        public void Init(string message)
        {
            text.text = message;//
        }
    }
}

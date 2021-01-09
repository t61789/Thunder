using Framework;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class MessageDialog : BaseUi
    {
        private Text text;

        protected override void Awake()
        {
            base.Awake();
            text = transform.Find("Text").GetComponent<Text>();
        }

        public void Init(string message)
        {
            text.text = message; //
        }
    }
}
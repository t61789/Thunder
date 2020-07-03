using Assets.Script.PublicScript;
using TMPro;

namespace Assets.Script.UI
{
    public class ConfirmDialog : BaseUi
    {
        public TextMeshProUGUI textMesh;

        public DialogResult dialogResult;

        private string tempText;

        protected override void Awake()
        {
            base.Awake();
            textMesh = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            tempText = textMesh.text;
        }

        public void Init(string text)
        {
            textMesh.SetText(text);
        }

        public void OK()
        {
            dialogResult = DialogResult.OK;
            PublicVar.UiSys.CloseUi(this);
        }

        public void Cancel()
        {
            dialogResult = DialogResult.Cancel;
            PublicVar.UiSys.CloseUi(this);
        }

        public void Update()
        {
            if (textMesh.text != tempText)
            {
                tempText = textMesh.text;
                RectTrans.SetSizeWithCurrentAnchors(UnityEngine.RectTransform.Axis.Horizontal, textMesh.rectTransform.rect.width);
            }
        }
    }
}

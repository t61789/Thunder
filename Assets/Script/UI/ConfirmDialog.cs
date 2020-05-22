using TMPro;

public class ConfirmDialog : BaseUI
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
        PublicVar.uiManager.CloseUI(this);
    }

    public void Cancel()
    {
        dialogResult = DialogResult.Cancel;
        PublicVar.uiManager.CloseUI(this);
    }

    public void Update()
    {
        if (textMesh.text != tempText)
        {
            tempText = textMesh.text;
            rectTrans.SetSizeWithCurrentAnchors(UnityEngine.RectTransform.Axis.Horizontal, textMesh.rectTransform.rect.width);
        }
    }
}

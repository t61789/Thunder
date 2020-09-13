using Thunder.Entity;
using TMPro;

namespace Thunder.UI
{
    public class AmmoPanel : BaseUI
    {
        private TextMeshProUGUI _Text;

        private void Start()
        {
            _Text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            Gun.Instance.OnAmmoChange.AddListener(SetText);
            Gun.Instance.BroadCastAmmo();
        }

        private void SetText(float max, float mag, float backup)
        {
            _Text.text = $"{mag}/{max}  {backup}";
        }
    }
}

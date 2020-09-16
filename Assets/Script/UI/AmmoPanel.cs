using Thunder.Entity;
using Thunder.Utility;
using TMPro;

namespace Thunder.UI
{
    public class AmmoPanel : BaseUI
    {
        private TextMeshProUGUI _Text;
        private string _AmmoStr;
        private string _FireModeStr;

        private void Start()
        {
            _Text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            Gun.Instance.OnAmmoChange.AddListener((max, mag, backup) =>
            {
                _AmmoStr = $"{mag}/{max}  {backup}";
                SetText();
            });
            PublicEvents.GunFireModeChange.AddListener(x =>
            {
                switch (x)
                {
                    case 1:
                        _FireModeStr = " i ";
                        break;
                    case 2:
                        _FireModeStr = "i i";
                        break;
                    case 3:
                        _FireModeStr = "iii";
                        break;
                    default:
                        _FireModeStr = " F ";
                        break;
                }
                SetText();
            });
            Gun.Instance.BroadCastAmmo();
        }

        private void SetText()
        {
            _Text.text = $"{_AmmoStr} {_FireModeStr}";
        }
    }
}

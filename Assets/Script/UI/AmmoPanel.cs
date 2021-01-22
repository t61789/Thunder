using Framework;
using TMPro;

namespace Thunder.UI
{
    public class AmmoPanel : BaseUi
    {
        private string _AmmoStr;
        private string _FireModeStr;
        private TextMeshProUGUI _Text;

        protected override void Awake()
        {
            PublicEvents.TakeOutWeapon.AddListener(BindWeapon);
            PublicEvents.PutBackWeapon.AddListener(UnBindWeapon);
            PublicEvents.GunFireModeChange.AddListener(UpdateGunFireMode);
            _Text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        }

        private void OnDestroy()
        {
            PublicEvents.TakeOutWeapon.RemoveListener(BindWeapon);
            PublicEvents.PutBackWeapon.RemoveListener(UnBindWeapon);
            PublicEvents.GunFireModeChange.RemoveListener(UpdateGunFireMode);
        }

        private void SetText()
        {
            _Text.text = $"{_AmmoStr} {_FireModeStr}";
        }

        private void BindWeapon(BaseWeapon weapon)
        {
            weapon.AmmoGroup.OnAmmoChanged += UpdateAmmo;
        }

        private void UnBindWeapon(BaseWeapon weapon)
        {
            weapon.AmmoGroup.OnAmmoChanged -= UpdateAmmo;
        }

        private void UpdateAmmo(AmmoGroup ammoGroup)
        {
            _AmmoStr = $"{ammoGroup.Magazine}/{ammoGroup.MagazineMax}  {ammoGroup.BackupAmmo}";
            SetText();
        }

        private void UpdateGunFireMode(int fireMode)
        {
            switch (fireMode)
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
        }
    }
}
﻿using Framework;
using TMPro;

namespace Thunder.UI
{
    public class AmmoPanel : BaseUi
    {
        private string _AmmoStr;
        private string _FireModeStr;
        private TextMeshProUGUI _Text;

        private void Start()
        {
            _Text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            BaseWeapon.Ins.AmmoGroup.OnAmmoChanged += ammoGroup =>
            {
                _AmmoStr = $"{ammoGroup.Magazine}/{ammoGroup.MagazineMax}  {ammoGroup.BackupAmmo}";
                SetText();
            };
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
            BaseWeapon.Ins.AmmoGroup.InvokeOnAmmoChanged();
        }

        private void SetText()
        {
            _Text.text = $"{_AmmoStr} {_FireModeStr}";
        }
    }
}
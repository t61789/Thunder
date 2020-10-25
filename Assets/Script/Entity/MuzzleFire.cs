using Thunder.Entity.Weapon;

using Thunder.Utility;
using Tool;
using UnityEngine;

namespace Thunder.Entity
{
    public class MuzzleFire : MonoBehaviour
    {
        private Light _FireLight;

        private AutoCounter _LifeTimeCounter;
        private SpriteRenderer _SpriteRenderer;
        public float LifeTime;
        public Sprite[] Sprites = new Sprite[0];

        private void Start()
        {
            _SpriteRenderer = GetComponent<SpriteRenderer>();
            _FireLight = transform.Find("fireLight").GetComponent<Light>();
            _FireLight.enabled = false;
            PublicEvents.GunFire.AddListener(Fire);
            _LifeTimeCounter = new AutoCounter(this, LifeTime).OnComplete(() =>
            {
                _SpriteRenderer.sprite = null;
                _FireLight.enabled = false;
            });
            Install();
        }

        private void Install()
        {
            var gun = (MachineGun) BaseWeapon.Ins;
            transform.localPosition = gun.MuzzleFirePos;
            Sprites = gun.MuzzleFireSprites;
        }

        private void Fire()
        {
            if (Sprites.Length == 0) return;
            _SpriteRenderer.sprite = Sprites.RandomTake();
            if (_LifeTimeCounter.Completed)
                _LifeTimeCounter.Recount();
            _FireLight.enabled = true;
        }
    }
}
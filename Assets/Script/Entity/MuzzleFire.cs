using Framework;
using UnityEngine;

namespace Thunder
{
    public class MuzzleFire : MonoBehaviour
    {
        private Light _FireLight;

        private SemiAutoCounter _LifeTimeCounter;
        private SpriteRenderer _SpriteRenderer;
        public float LifeTime;

        private Sprite[] _Sprites = new Sprite[0];

        private void Awake()
        {
            PublicEvents.GunFire.AddListener(Fire);
            PublicEvents.TakeOutWeapon.AddListener(Install);
            PublicEvents.PutBackWeapon.AddListener(UnInstall);

            _SpriteRenderer = GetComponent<SpriteRenderer>();
            _FireLight = transform.Find("fireLight").GetComponent<Light>();
            _FireLight.enabled = false;
            _LifeTimeCounter = new SemiAutoCounter(LifeTime).OnComplete(() =>
            {
                _SpriteRenderer.sprite = null;
                _FireLight.enabled = false;
            });
        }

        private void OnDestroy()
        {
            PublicEvents.GunFire.RemoveListener(Fire);
            PublicEvents.TakeOutWeapon.RemoveListener(Install);
            PublicEvents.PutBackWeapon.RemoveListener(UnInstall);
        }

        private void FixedUpdate()
        {
            _LifeTimeCounter.FixedUpdate();
        }

        private void Install(BaseWeapon weapon)
        {
            var gun = weapon as MachineGun;
            if (gun == null) return;
            transform.localPosition = gun.MuzzleFirePos;
            _Sprites = gun.MuzzleFireSprites;
        }

        private void UnInstall(BaseWeapon weapon)
        {
            _Sprites = null;
        }

        private void Fire()
        {
            if (_Sprites.Length == 0) return;
            _LifeTimeCounter.Recount();

            _SpriteRenderer.sprite = _Sprites.RandomTake();
            _FireLight.enabled = true;
        }
    }
}
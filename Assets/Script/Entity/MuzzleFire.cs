﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Tool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity
{
    public class MuzzleFire : MonoBehaviour
    {
        public float LifeTime;
        public Sprite[] Sprites = new Sprite[0];

        private Counter _LifeTimeCounter;
        private SpriteRenderer _SpriteRenderer;
        private Light _FireLight;

        private void Start()
        {
            _SpriteRenderer = GetComponent<SpriteRenderer>();
            _FireLight = transform.Find("fireLight").GetComponent<Light>();
            _FireLight.enabled = false;
            PublicEvents.GunFire.AddListener(Fire);
            _LifeTimeCounter = new Counter(LifeTime).OnComplete(() =>
            {
                _SpriteRenderer.sprite = null;
                _FireLight.enabled = false;
            }).ToAutoCounter(this);
            Install();
        }

        private void Install()
        {
            transform.localPosition = Gun.Instance.MuzzleFirePos;
            Sprites = Gun.Instance.MuzzleFireSprites;
        }

        private void Fire()
        {
            if (Sprites.Length == 0) return;
            _SpriteRenderer.sprite = Sprites.RandomTake();
            if(_LifeTimeCounter.Completed)
                _LifeTimeCounter.Recount();
            _FireLight.enabled = true;
        }
    }
}
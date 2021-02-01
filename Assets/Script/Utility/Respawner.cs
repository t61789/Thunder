using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using UnityEngine;

namespace Thunder
{
    public class Respawner:BaseCharacter
    {
        public float RespawnTime;
        public string PrefabPath;
        
        private Func<float> _DifficultyFactor;
        private SimpleCounter _RespawnCounter;

        public void Init(Func<float> getDifficulty)
        {
            if (PrefabPath.IsNullOrEmpty())
                throw new Exception($"重生点 {name} 未指定prefab");

            _DifficultyFactor = getDifficulty;
            _RespawnCounter = new SimpleCounter(RespawnTime);
        }

        private void FixedUpdate()
        {
            if (_RespawnCounter.Interpolant * (1 / _DifficultyFactor()) >= 1)
            {
                _RespawnCounter.Recount();

                var character = GameObjectPool.Get<BaseCharacter>(PrefabPath);
                character.Trans.position = Trans.position;
            }
        }
    }
}

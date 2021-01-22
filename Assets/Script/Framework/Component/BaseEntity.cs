using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Framework
{
    public class BaseEntity : MonoBehaviour
    {
        [SerializeField] private string _EntityName;

        public string EntityName
        {
            set => _EntityName = value;

            get => string.IsNullOrEmpty(_EntityName) ? name : _EntityName;
        }

        public Transform Trans { protected set; get; }

        protected virtual void Awake()
        {
            Trans = transform;
        }
    }

    [Serializable]
    public class Health
    {
        public float MaxHealth = 100;
        public float CurHealth = 100;

        public event Action OnDead;

        public void Init()
        {

        }

        /// <summary>
        /// 消耗指定数量的体力值
        /// </summary>
        /// <param name="num"></param>
        /// <returns>未满足消耗的体力值</returns>
        public float CostHealth(float num)
        {
            CurHealth -= num;
            if (CurHealth <= 0)
            {
                var result = -CurHealth;
                CurHealth = 0;
                InvokeOnDead();
                return result;
            }

            return 0;
        }

        /// <summary>
        /// 回复指定数量的体力值
        /// </summary>
        /// <param name="num"></param>
        /// <returns>过量回复的体力值的体力值</returns>
        public float RecoverHealth(float num)
        {
            CurHealth += num;
            if (CurHealth > MaxHealth)
            {
                var result = CurHealth - MaxHealth;
                CurHealth = MaxHealth;
                return result;
            }

            return 0;
        }

        protected virtual void InvokeOnDead()
        {
            OnDead?.Invoke();
        }
    }
}
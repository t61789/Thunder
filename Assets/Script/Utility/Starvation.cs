using System.Collections.Generic;
using Framework;
using UnityEngine;
using UnityEngine.Events;

namespace Thunder
{
    public class Starvation : MonoBehaviour
    {
        public bool CanStarve = true;

        [Tooltip("经过多少时间后完全饥饿/秒")]
        [SerializeField]
        private float _StarvationTime = 900;
        private float _StarvationReduceValueBase;
        private readonly LinkedList<RecoverStarvationUnit> _RecoverStarvationUnits 
            = new LinkedList<RecoverStarvationUnit>();

        public UnityEvent<Starvation> OnCompletelyStarvation { get; } = new UnityEvent<Starvation>();
        public float StarvationValue { private set; get; } = 1;
        public BuffData StarvationFactor { get; } = new BuffData(1);
        private float StarvationReduceValue => _StarvationReduceValueBase * StarvationFactor;

        private void Awake()
        {
            _StarvationReduceValueBase = Time.fixedDeltaTime / _StarvationTime;
        }

        private void FixedUpdate()
        {
            LifeCycle();
        }

        public void RecoverStarvationDuration(float duration, float amount)
        {
            _RecoverStarvationUnits.AddLast(
                new LinkedListNode<RecoverStarvationUnit>(
                    new RecoverStarvationUnit(duration,amount)));
        }

        public void RecoverStarvationImmediatelty(float amount)
        {
            GetRecover(amount);
        }

        private void LifeCycle()
        {
            StarveCycle();
            RecoverCycle();
        }

        private void StarveCycle()
        {
            if (!CanStarve || _RecoverStarvationUnits.Count != 0) return;
            var newStarvationValue = (StarvationValue - StarvationReduceValue).Clamp01();
            if (StarvationValue != 0 && newStarvationValue == 0)
                OnCompletelyStarvation?.Invoke(this);
            StarvationValue = newStarvationValue;
        }

        private void RecoverCycle()
        {
            var node = _RecoverStarvationUnits.First;
            while (node != null)
            {
                var newNode = node.Next;
                var unit = node.Value;
                if (unit.Counter.Completed)
                    _RecoverStarvationUnits.Remove(node);
                else
                    GetRecover(unit.RecoverValue);
                node = newNode;
            }
        }

        private void GetRecover(float amount)
        {
            StarvationValue = (StarvationValue + amount).Clamp01();
        }

        private readonly struct RecoverStarvationUnit
        {
            public readonly SimpleCounter Counter;
            public readonly float RecoverValue;

            public RecoverStarvationUnit(float duration, float amount)
            {
                Counter = new SimpleCounter(duration);
                RecoverValue = (duration / amount) * Time.fixedDeltaTime;
            }
        }
    }
}

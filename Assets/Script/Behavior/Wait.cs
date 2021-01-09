using BehaviorDesigner.Runtime.Tasks;
using Framework;
using UnityEngine;

namespace Thunder.Behavior
{
    public class Wait:Action
    {
        public float Time;

        private SimpleCounter _Counter;
        private bool _CountEnd=true;

        public override void OnAwake()
        {
            _Counter = new SimpleCounter(Time);
        }

        public override TaskStatus OnUpdate()
        {
            if (_CountEnd)
            {
                _Counter.Recount();
                _CountEnd = false;
            }
            if (!_Counter.Completed) return TaskStatus.Running;
            _CountEnd = true;
            return TaskStatus.Success;
        }
    }
}

using System;
using UnityEngine;

namespace Thunder.Utility
{
    [Serializable]
    public class SimpleCounter : Counter
    {
        public SimpleCounter(float timeLimit, bool countAtStart = true) : base(timeLimit)
        {
            if (countAtStart) return;
            _TimeCountStart -= _TimeLimit;
        }

        public override float TimeCount =>
            Time.time - _TimeCountStart;

        public override bool Completed => Time.time >= _TimeCountStart + _TimeLimit;

        public override void Recount(float timeLimit = -1)
        {
            _TimeLimit = timeLimit == -1 ? _TimeLimit : timeLimit;
            _TimeCountStart = Time.time;
        }

        public override void SetCountValue(float factor)
        {
            _TimeCountStart = Time.time - factor * TimeLimit;
        }

        /// <summary>
        ///     立即完成计时
        /// </summary>
        /// <returns></returns>
        public SimpleCounter Complete()
        {
            _TimeCountStart = Time.time - _TimeLimit;
            return this;
        }
    }
}
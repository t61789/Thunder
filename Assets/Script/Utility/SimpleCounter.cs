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

        /// <summary>
        ///     重新计时
        /// </summary>
        /// <param name="timeLimit">新的计时时限，为-1则不做改变</param>
        /// <returns></returns>
        public SimpleCounter Recount(float timeLimit = -1)
        {
            _TimeLimit = timeLimit == -1 ? _TimeLimit : timeLimit;
            _TimeCountStart = Time.time;
            return this;
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
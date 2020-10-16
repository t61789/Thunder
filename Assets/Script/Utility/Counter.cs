﻿using Thunder.Tool;
using UnityEngine;

namespace Thunder.Utility
{
    public abstract class Counter
    {
        protected float _TimeCountStart;

        protected float _TimeLimit;

        protected Counter(float timeLimit)
        {
            _TimeLimit = timeLimit;
            _TimeCountStart = Time.time;
        }

        /// <summary>
        ///     当前计时与目标时间的插值，经过clamp处理
        /// </summary>
        public float Interpolant =>
            Tools.InLerp(0, _TimeLimit, TimeCount);

        /// <summary>
        ///     当前计时与目标时间的插值，未经clamp处理
        /// </summary>
        public float InterpolantUc =>
            Tools.InLerpUc(0, _TimeLimit, TimeCount);

        /// <summary>
        ///     当前已经记录了多少时间
        /// </summary>
        public abstract float TimeCount { get; }

        /// <summary>
        ///     指示是否已经完成计时
        /// </summary>
        public abstract bool Completed { get; }

        /// <summary>
        ///     计时上限
        /// </summary>
        public float TimeLimit => _TimeLimit;

        /// <summary>
        /// 重新计数
        /// </summary>
        /// <param name="timeLimit">新的计数时限，若为-1则不做改动</param>
        public abstract void Recount(float timeLimit=-1);

        /// <summary>
        /// 根据指定的factor设置当前计数器的计时起点
        /// </summary>
        /// <param name="interpolant"></param>
        public abstract void SetCountValue(float factor);
    }
}
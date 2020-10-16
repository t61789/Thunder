using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Utility
{
    /// <summary>
    /// 半自动计时器，使用提供的Update和FixedUpdate实现自动执行回调函数，而非协程<br/>
    /// 用法大体与自动计时器相同，但对性能更友好，适用于大量简单物体的计时
    /// </summary>
    public class SemiAutoCounter:Counter
    {
        private float _CountPauseSave;
        private bool _HasExcutedCompleteCallBack;
        private Action _OnCompleteCallBack;
        private bool _Running=true;

        /// <summary>
        /// </summary>
        /// <param name="parent">协程附着对象</param>
        /// <param name="timeLimit"></param>
        /// <param name="countAtStart"></param>
        public SemiAutoCounter(float timeLimit) : base(timeLimit)
        {
            _TimeCountStart = Time.time;
        }

        public override float TimeCount
        {
            get
            {
                if (!_Running)
                    return _CountPauseSave;
                return Time.time - _TimeCountStart;
            }
        }

        public override bool Completed => _HasExcutedCompleteCallBack;

        public override void Recount(float timeLimit = -1)
        {
            _TimeLimit = timeLimit == -1 ? _TimeLimit : timeLimit;
            _TimeCountStart = Time.time;
            _HasExcutedCompleteCallBack = false;
            _CountPauseSave = 0;
        }

        /// <summary>
        ///     立即完成计时
        /// </summary>
        /// <param name="callback">是否调用完成回调函数</param>
        /// <returns></returns>
        public SemiAutoCounter Complete(bool callback=true)
        {
            _TimeCountStart = Time.time - _TimeLimit;
            if (callback)
                _OnCompleteCallBack?.Invoke();
            _HasExcutedCompleteCallBack = true;
            _CountPauseSave = _TimeLimit;
            return this;
        }

        /// <summary>
        ///     启用自动计时后调用该回调函数
        /// </summary>
        /// <param name="callBack">回调函数</param>
        /// <returns></returns>
        public SemiAutoCounter OnComplete(Action callBack)
        {
            _OnCompleteCallBack = callBack;
            return this;
        }

        /// <summary>
        ///     恢复自动计时，如果当前正在自动计时，则重置计数
        /// </summary>
        /// <returns></returns>
        public SemiAutoCounter Resume()
        {
            _Running = true;
            _TimeCountStart = Time.time - _CountPauseSave;
            return this;
        }

        /// <summary>
        ///     暂停自动计时
        /// </summary>
        /// <returns></returns>
        public SemiAutoCounter Pause()
        {
            _Running = false;
            _CountPauseSave = Time.time - _TimeCountStart;
            return this;
        }
        
        public void Update()
        {
            if (_HasExcutedCompleteCallBack || !_Running || Time.time <= _TimeCountStart + _TimeLimit) return;
            _HasExcutedCompleteCallBack = true;
            _OnCompleteCallBack?.Invoke();
        }

        public void FixedUpdate()
        {
            if (_HasExcutedCompleteCallBack || !_Running || Time.time <= _TimeCountStart + _TimeLimit) return;
            _HasExcutedCompleteCallBack = true;
            _OnCompleteCallBack?.Invoke();
        }

        public override void SetCountValue(float factor)
        {
            if (_Running)
                _TimeCountStart = Time.time - factor * TimeLimit;
            else
                _CountPauseSave = TimeLimit * factor;

            if (factor <= 0)
                _HasExcutedCompleteCallBack = false;
        }
    }

    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    public class SemiAutoCounterHub
    {
        private readonly List<SemiAutoCounter> _Counters;

        public SemiAutoCounterHub(params SemiAutoCounter[] counters)
        {
            _Counters = new List<SemiAutoCounter>(counters);
        }

        public void Update()
        {
            for(int i=0;i<_Counters.Count;i++)
                _Counters[i].Update();
        }

        public void FixedUpdate()
        {
            for (int i = 0; i < _Counters.Count; i++)
                _Counters[i].FixedUpdate();
        }

        public void AddCounter(SemiAutoCounter counter)
        {
            _Counters.Add(counter);
        }

        public void RemoveCounter(SemiAutoCounter counter)
        {
            _Counters.Remove(counter);
        }
    }
}

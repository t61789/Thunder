﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Utility
{
    public class AutoCounter:Counter
    {
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

        private float _CountPauseSave;
        private Action _OnCompleteCallBack;
        private bool _HasExcutedCompleteCallBack;
        private bool _Running;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">协程附着对象</param>
        /// <param name="timeLimit"></param>
        /// <param name="countAtStart"></param>
        public AutoCounter(MonoBehaviour parent,float timeLimit, bool countAtStart = true) : base(timeLimit, countAtStart)
        {
            _TimeCountStart = Time.time;
            _Running = countAtStart;
            parent.StartCoroutine(Count());
        }

        public override Counter Recount(float timeLimit = -1)
        {
            _TimeLimit = timeLimit == -1 ? _TimeLimit : timeLimit;
            _TimeCountStart = Time.time;
            _HasExcutedCompleteCallBack = false;
            _CountPauseSave = 0;
            return this;
        }

        public override Counter Complete()
        {
            return Complete(true);
        }

        public AutoCounter Complete(bool callback)
        {
            _TimeCountStart = Time.time - _TimeLimit;
            if (callback)
                _OnCompleteCallBack?.Invoke();
            _HasExcutedCompleteCallBack = true;
            _CountPauseSave = _TimeLimit;
            return this;
        }

        /// <summary>
        /// 启用自动计时后调用该回调函数
        /// </summary>
        /// <param name="callBack">回调函数</param>
        /// <returns></returns>
        public AutoCounter OnComplete(Action callBack)
        {
            _OnCompleteCallBack = callBack;
            return this;
        }

        /// <summary>
        /// 恢复自动计时，如果当前正在自动计时，则重置计数
        /// </summary>
        /// <returns></returns>
        public AutoCounter Resume()
        {
            _Running = true;
            _TimeCountStart = Time.time - _CountPauseSave;
            return this;
        }

        /// <summary>
        /// 暂停自动计时
        /// </summary>
        /// <returns></returns>
        public AutoCounter Pause()
        {
            _Running = false;
            _CountPauseSave = Time.time - _TimeCountStart;
            return this;
        }

        private IEnumerator Count()
        {
            while (true)
            {
                if (!_HasExcutedCompleteCallBack && _Running && Time.time >= _TimeCountStart + _TimeLimit)
                {
                    _HasExcutedCompleteCallBack = true;
                    _OnCompleteCallBack?.Invoke();
                }

                yield return null;
            }
        }
    }
}
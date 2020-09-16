using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using Thunder.Tool;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Utility
{
    [Serializable]
    public class Counter
    {
        /// <summary>
        /// 当前计时与目标时间的插值，未经clamp处理
        /// </summary>
        public float Interpolant =>
            Tools.InLerpUc(_TimeCount, _TimeCount + TimeLimit, Time.time);

        /// <summary>
        /// 当前已经距离上次重新计时过去了多少秒
        /// </summary>
        public float TimeCount =>
            Time.time - _TimeCount;

        /// <summary>
        /// 指示是否已经完成计时
        /// </summary>
        public bool Completed => Time.time >= _TimeCount + TimeLimit;
        
        /// <summary>
        /// 计时时限
        /// </summary>
        public float TimeLimit;

        /// <summary>
        /// 指示是否在自动计时
        /// </summary>
        public bool AutoCount { get; private set; }

        /// <summary>
        /// 指示计时器是否是自动计时器
        /// </summary>
        public bool IsAutoCounter => _Coroutine != null;

        private float _TimeCount;
        private Action _OnCompleteCallBack;
        private bool _Completed;
        private Coroutine _Coroutine;
        private float _CountPauseSave;

        /// <summary>
        /// 创建一个计时器，初始为被动计时器
        /// </summary>
        /// <param name="timeLimit">计时时限</param>
        /// <param name="countAtStart">是否在创建完成后立即开始计时</param>
        public Counter(float timeLimit,bool countAtStart=true)
        {
            TimeLimit = timeLimit;
            _TimeCount = Time.time;
            if (countAtStart) return;
            _Completed = true;
            _TimeCount -= TimeLimit;
        }

        /// <summary>
        /// 重新计时
        /// </summary>
        /// <param name="timeLimit">新的计时时限，为-1则不做改变</param>
        /// <returns></returns>
        public Counter Recount(float timeLimit = -1)
        {
            TimeLimit = timeLimit == -1 ? TimeLimit : timeLimit;
            _TimeCount = Time.time;
            _Completed = false;
            return this;
        }

        /// <summary>
        /// 启用自动计时后调用该回调函数
        /// </summary>
        /// <param name="callBack">回调函数</param>
        /// <returns></returns>
        public Counter OnComplete(Action callBack)
        {
            _OnCompleteCallBack = callBack;
            return this;
        }

        /// <summary>
        /// 立即完成计时
        /// </summary>
        /// <param name="callback">是否调用完成回调函数</param>
        /// <returns></returns>
        public Counter Complete(bool callback=false)
        {
            _TimeCount = Time.time - TimeLimit;
            if(callback)
                _OnCompleteCallBack?.Invoke();
            _Completed = true;
            return this;
        }

        /// <summary>
        /// 将被动计时器转化为自动计时器
        /// </summary>
        /// <param name="parent">依附对象</param>
        /// <param name="start"></param>
        /// <returns></returns>
        public Counter ToAutoCounter(MonoBehaviour parent,bool start=true)
        {
            Assert.IsFalse(IsAutoCounter,"当前已经是自动计时器");
            _Coroutine = parent.StartCoroutine(Count());
            AutoCount = start;
            _TimeCount = Time.time;
            return this;
        }

        /// <summary>
        /// 将自动计时器转化为被动计时器
        /// </summary>
        /// <param name="parent">依附对象</param>
        /// <returns></returns>
        public Counter ToNegetiveCounter(MonoBehaviour parent)
        {
            Assert.IsTrue(IsAutoCounter, "当前已经是被动计时器");
            _CountPauseSave = 0;
            _TimeCount = Time.time;
            parent.StopCoroutine(_Coroutine);
            _Coroutine = null;
            AutoCount = false;
            return this;
        }

        /// <summary>
        /// 恢复自动计时，如果当前正在自动计时，则重置计数
        /// </summary>
        /// <param name="recount">是否重新计时，即清除上一次暂停之前所记录的时间。反之则继续上一次的计时</param>
        /// <returns></returns>
        public Counter ResumeAutoCount(bool recount=false)
        {
            if (AutoCount || recount)
            {
                _CountPauseSave = 0;
                _Completed = false;
            }
            AutoCount = true;
            _TimeCount = Time.time - _CountPauseSave;
            return this;
        }

        /// <summary>
        /// 暂停自动计时
        /// </summary>
        /// <returns></returns>
        public Counter PauseAutoCount()
        {
            Assert.IsTrue(AutoCount, "自动计时未开启，请先启动");
            AutoCount = false;
            _CountPauseSave = Time.time - _TimeCount;
            return this;
        }

        private IEnumerator Count()
        {
            while (true)
            {
                if (!_Completed && AutoCount && Time.time >= _TimeCount + TimeLimit)
                {
                    _Completed = true;
                    _OnCompleteCallBack?.Invoke();
                }
                yield return null;
            }
        }
    }
}

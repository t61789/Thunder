using System;
using System.Collections;
using Thunder.Tool;
using UnityEngine;

namespace Thunder.Utility
{
    [Serializable]
    public class Counter
    {
        private struct AvaliableData
        {
            public float Float;
            public Vector3 Vector;
            public Color Color;
        }

        /// <summary>
        /// 当前时间计数与目标时间的插值，未经clamp处理
        /// </summary>
        public float Interpolant =>
            Tools.InLerpUc(_TimeCount, _TimeCount + TimeLimit, Time.time);

        /// <summary>
        /// 当前已经距离上次重新计数过去了多少秒
        /// </summary>
        public float TimeCount =>
            Time.time - _TimeCount;

        public float LerpFloat =>
            Tools.Lerp(_LValue.Float, _RValue.Float, Interpolant);

        public float LerpFloatUc => 
            Tools.LerpUc(_LValue.Float,_RValue.Float,Interpolant);

        public Vector3 LerpVector =>
            Tools.Lerp(_LValue.Vector, _RValue.Vector, Interpolant);

        public Vector3 LerpVectorUc =>
            Tools.LerpUc(_LValue.Vector, _RValue.Vector, Interpolant);

        public Color LerpColor =>
            Tools.Lerp(_LValue.Color, _RValue.Color, Interpolant);

        public Color LerpColorUc =>
            Tools.LerpUc(_LValue.Color, _RValue.Color, Interpolant);

        public float FloatL
        {
            get => _LValue.Float;
            set => _LValue.Float = value;
        }
        
        public float FloatR
        {
            get => _RValue.Float;
            set => _RValue.Float = value;
        }

        public Vector3 VectorL
        {
            get => _LValue.Vector;
            set => _LValue.Vector = value;
        }

        public Vector3 VectorR
        {
            get => _RValue.Vector;
            set => _RValue.Vector = value;
        }

        public Color ColorL
        {
            get => _LValue.Color;
            set => _LValue.Color = value;
        }

        public Color ColorR
        {
            get => _RValue.Color;
            set => _RValue.Color = value;
        }

        private float _TimeCount;
        
        public float TimeLimit;
        private Action _OnCompleteCallBack;
        private bool _Completed;
        private Coroutine _Coroutine;

        private AvaliableData _LValue;
        private AvaliableData _RValue;

        public Counter(float timeLimit,bool countAtStart=true)
        {
            TimeLimit = timeLimit;
            _TimeCount = Time.time;
            if (countAtStart) return;
            _Completed = true;
            _TimeCount -= TimeLimit;
        }

        public void Recount()
        {
            _TimeCount = Time.time;
            _Completed = false;
        }

        public Counter OnComplete(Action callBack)
        {
            _OnCompleteCallBack = callBack;
            return this;
        }

        public IEnumerator Count()
        {
            while (true)
            {
                if (!_Completed && Time.time >= _TimeCount + TimeLimit)
                {
                    _Completed = true;
                    _OnCompleteCallBack?.Invoke();
                }
                yield return null;
            }
        }

        public Counter StartCount(MonoBehaviour parent)
        {
            if (_Coroutine != null) return this;
            _Coroutine = parent.StartCoroutine(Count());
            return this;
        }

        public Counter StopCount(MonoBehaviour parent)
        {
            if (_Coroutine == null) return this;
            parent.StopCoroutine(_Coroutine);
            _Coroutine = null;
            return this;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.Assertions;

namespace Thunder.Utility
{
    public class CircleBuffer<T>:IEnumerator<T>
    {
        /// <summary>
        /// 缓冲区的大小
        /// </summary>
        public int LogBufferSize
        {
            get => _Buffer.Length;

            set
            {
                if (value <= LogBufferSize || value<2)
                    return;

                var newBuffer = new T[value];
                if (_BufferPointer.end < _BufferPointer.start)
                {
                    int newEndPointer = _Buffer.Length - _BufferPointer.start;
                    Array.Copy(
                        _Buffer,
                        _BufferPointer.start,
                        newBuffer,
                        0,
                        newEndPointer);
                    int endLength = _BufferPointer.end + 1;
                    Array.Copy(
                        _Buffer,
                        0,
                        newBuffer,
                        newEndPointer,
                        endLength);
                    _BufferPointer.start = 0;
                    _BufferPointer.end = newEndPointer + endLength-1;
                }
                else
                {
                    int arrLength = LogBufferSize;
                    Array.Copy(
                        _Buffer,
                        _BufferPointer.start,
                        newBuffer,
                        0,
                        arrLength);
                    _BufferPointer.start = 0;
                    _BufferPointer.end = arrLength - 1;
                }

                _Buffer = newBuffer;
            }
        }

        /// <summary>
        /// 缓冲区中元素的数量
        /// </summary>
        public int ElementSize { get; private set; } = 0;

        private T[] _Buffer;
        private (int start, int end) _BufferPointer = (0,0);
        private int _Version;
        private int _StartVersion;
        private int _EnumIndex;

        public T this[int index]
        {
            get
            {
                if(index<0||index>=LogBufferSize)
                    throw new ArgumentOutOfRangeException();
                return _Buffer[LimitPointer(_BufferPointer.start + index)];
            }

            set
            {
                if (index < 0 || index >= LogBufferSize)
                    throw new ArgumentOutOfRangeException();
                _Buffer[LimitPointer(_BufferPointer.start + index)] = value;
            }
        }

        public CircleBuffer(int bufferSize)
        {
            Assert.IsTrue(bufferSize>1,"缓冲区大小不正确，需要 size>0");
            _Buffer = new T[bufferSize];
        }

        /// <summary>
        /// 在环形缓冲区的队首或队尾添加元素
        /// </summary>
        /// <param name="value"></param>
        /// <param name="end">为true则在队尾添加元素，否则在队首添加</param>
        /// <param name="cautious">指示当缓冲区满后是否自动覆盖 队首/队尾 的元素</param>
        /// <returns>当缓冲区已满时返回被覆盖的元素</returns>
        public T Insert(T value,bool end=true,bool cautious=false)
        {
            if (ElementSize == 0)
            {
                _Buffer[_BufferPointer.start] = value;
                ElementSize++;
                _Version++;
                return default;
            }

            T result;
            if (end)
            {
                int endLimit = LimitPointer(_BufferPointer.end + 1);
                if (cautious && endLimit == _BufferPointer.start)
                    return default;
                result = default;
                if (endLimit == _BufferPointer.start)
                    result = _Buffer[endLimit];
                _BufferPointer.end = endLimit;
                _Buffer[_BufferPointer.end] = value;

                if (_BufferPointer.end == _BufferPointer.start)
                {
                    _BufferPointer.start = LimitPointer(_BufferPointer.start + 1);
                }
                else
                    ElementSize++;
                _Version++;
                return result;
            }

            int startLimit = LimitPointer(_BufferPointer.start - 1);
            if (cautious && startLimit == _BufferPointer.end)
                return default;
            result = default;
            if (startLimit == _BufferPointer.end)
                result = _Buffer[startLimit];
            _BufferPointer.start = startLimit;
            _Buffer[_BufferPointer.start] = value;
            if (_BufferPointer.end == _BufferPointer.start)
                _BufferPointer.end = LimitPointer(_BufferPointer.end - 1);
            else
                ElementSize++;
            _Version++;
            return result;
        }

        /// <summary>
        /// 移除队首/队尾的元素
        /// </summary>
        /// <param name="start">为true则在队首移除，否则在队尾移除</param>
        /// <param name="clear">是否清除单元格内的元素</param>
        /// <returns>被移除的元素</returns>
        public T Remove(bool start = true, bool clear = true)
        {
            switch (ElementSize)
            {
                case 0:
                    return default;
                case 1:
                {
                    if (clear)
                        _Buffer[_BufferPointer.end] = default;
                    ElementSize--;
                    _Version++;
                    return default;
                }
            }

            T result;
            if (!start)
            {
                result = _Buffer[_BufferPointer.end];
                if (clear)
                    _Buffer[_BufferPointer.end] = default;
                _BufferPointer.end = LimitPointer(_BufferPointer.end - 1);
            }
            else
            {
                result = _Buffer[_BufferPointer.start];
                if (clear)
                    _Buffer[_BufferPointer.start] = default;
                _BufferPointer.start = LimitPointer(_BufferPointer.start + 1);
            }

            ElementSize--;
            _Version++;
            return result;
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void RemoveAll()
        {
            for (int i = _BufferPointer.start; ; i = LimitPointer(i + 1))
            {
                _Buffer[i] = default;
                if (i != _BufferPointer.end) continue;
                _Version++;
                return;
            }
        }

        /// <summary>
        /// 将缓冲区大小设置为当前元素的数量
        /// </summary>
        public void Ensure()
        {
            int size = ElementSize;
            LogBufferSize = size;
        }

        private int LimitPointer(int p)
        {
            if (p >= 0) return p % _Buffer.Length;
            p %= _Buffer.Length;
            p += _Buffer.Length;
            return p;
        }

        public bool MoveNext()
        {
            Assert.IsTrue(_Version == _StartVersion,"不可在迭代时更改缓冲区内容");
            if (_EnumIndex == _BufferPointer.end) return false;
            _EnumIndex = LimitPointer(_EnumIndex + 1);
            return true;
        }

        public void Reset()
        {
            _EnumIndex = -1;
            _StartVersion = _Version;
        }

        public T Current => _Buffer[_EnumIndex];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

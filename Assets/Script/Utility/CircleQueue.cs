using System;
using UnityEngine.Assertions;

namespace Thunder.Utility
{
    public class CircleQueue<T>
    {
        private T[] _Buffer;
        private int _Head;
        private int _Tail;
        private int _Version;

        public CircleQueue(int bufferSize)
        {
            Assert.IsTrue(bufferSize >= 0, $"缓冲区大小不正确：{bufferSize}");
            _Buffer = new T[bufferSize];
        }

        /// <summary>
        ///     缓冲区的大小
        /// </summary>
        public int BufferSize
        {
            get => _Buffer.Length;

            set
            {
                if (value <= Count || value <= 0)
                    return;

                var newBuffer = new T[value];
                if (_Tail > _Head)
                {
                    var tempLength = _Buffer.Length - _Tail;
                    Array.Copy(
                        _Buffer,
                        _Tail,
                        newBuffer,
                        0,
                        tempLength);
                    Array.Copy(
                        _Buffer,
                        0,
                        newBuffer,
                        tempLength,
                        Count - tempLength);
                    _Tail = 0;
                    _Head = Count - 1;
                }
                else
                {
                    Array.Copy(
                        _Buffer,
                        _Tail,
                        newBuffer,
                        0,
                        Count);
                    _Tail = 0;
                    _Head = Count - 1;
                }

                _Buffer = newBuffer;
            }
        }

        /// <summary>
        ///     缓冲区中元素的数量
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="reverse">为false则从队首开始计算，否则从队尾开始</param>
        /// <returns></returns>
        public T this[int index, bool reverse = false]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();
                index = reverse ? _Tail + index : _Head - index;
                return _Buffer[LimitPointer(index)];
            }

            set
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();
                index = reverse ? _Tail + index : _Head - index;
                _Buffer[LimitPointer(index)] = value;
            }
        }

        /// <summary>
        ///     在环形缓冲区的队尾添加元素
        /// </summary>
        /// <param name="value"></param>
        /// <param name="reverse">为true为普通队列操作，否则反向</param>
        /// <param name="cautious">指示当缓冲区满后是否避免覆盖队首的元素</param>
        /// <returns>当缓冲区已满时返回被覆盖的元素</returns>
        public T Enqueue(T value, bool reverse = false, bool cautious = false)
        {
            if (Count == 0)
            {
                _Buffer[_Tail] = value;
                Count++;
                _Version++;
                return default;
            }

            T result;
            if (!reverse)
            {
                var tailLimited = LimitPointer(_Tail - 1);
                result = default;
                if (tailLimited == _Head)
                    if (cautious)
                    {
                        return default;
                    }
                    else
                    {
                        result = _Buffer[tailLimited];
                        _Head = LimitPointer(_Head + 1);
                        Count--;
                    }

                _Tail = tailLimited;
                _Buffer[_Tail] = value;
                Count++;
                _Version++;
                return result;
            }

            var headLimited = LimitPointer(_Head + 1);
            result = default;
            if (headLimited == _Tail)
                if (cautious)
                {
                    return default;
                }
                else
                {
                    result = _Buffer[headLimited];
                    _Tail = LimitPointer(_Tail - 1);
                    Count--;
                }

            _Head = headLimited;
            _Buffer[_Head] = value;
            Count++;
            _Version++;
            return result;
        }

        /// <summary>
        ///     移除队首的元素
        /// </summary>
        /// <param name="reverse">为true为普通队列操作，否则反向</param>
        /// <param name="clear">是否清除单元格内的元素</param>
        /// <returns>被移除的元素</returns>
        public T Dequeue(bool reverse = false, bool clear = true)
        {
            if (Count == 0) return default;
            T result;
            if (!reverse)
            {
                result = _Buffer[_Head];
                if (clear)
                    _Buffer[_Head] = default;
                if (Count != 1)
                    _Head = LimitPointer(_Head - 1);
            }
            else
            {
                result = _Buffer[_Tail];
                if (clear)
                    _Buffer[_Tail] = default;
                if (Count != 1)
                    _Tail = LimitPointer(_Tail + 1);
            }

            Count--;
            _Version++;
            return result;
        }

        /// <summary>
        ///     查看队首元素
        /// </summary>
        /// <param name="reverse">为true为普通队列操作，否则反向</param>
        /// <returns></returns>
        public T Peek(bool reverse)
        {
            return _Buffer[reverse ? _Tail : _Head];
        }

        /// <summary>
        ///     清空缓存
        /// </summary>
        public void RemoveAll()
        {
            while (true)
            {
                _Buffer[_Tail] = default;
                if (_Tail == _Head) break;
                _Tail = LimitPointer(_Tail + 1);
            }

            _Version++;
            Count = 0;
        }

        /// <summary>
        ///     将缓冲区大小设置为当前元素的数量
        /// </summary>
        public void Ensure()
        {
            BufferSize = Count;
        }

        private int LimitPointer(int p)
        {
            if (p >= 0) return p % _Buffer.Length;
            p %= _Buffer.Length;
            p += _Buffer.Length;
            return p;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Thunder.Sys;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Utility
{
    /// <summary>
    ///     用于在Update中读取数据后传递到FixedUpdate中
    /// </summary>
    public struct InputSynchronizer
    {
        private ControlInfo _Info;

        public void Set(ControlInfo value)
        {
            if (value.Down) _Info.Down = true;
            if (value.Stay) _Info.Stay = true;
            if (value.Up) _Info.Up = true;
            if (value.Axis != Vector3.zero) _Info.Axis = value.Axis;
        }

        public ControlInfo Get()
        {
            var result = _Info;
            _Info = new ControlInfo();
            return result;
        }
    }

    /// <summary>
    /// 将属性或成员的访问器转换成委托
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct GetterSetter<T>
    {
        public readonly Func<T> Get;
        public readonly Action<T> Set;

        public GetterSetter(Func<T> get, Action<T> set)
        {
            Get = get;
            Set = set;
        }
    }

    /// <summary>
    /// 环形队列，可在空间不足时自动覆盖尾部数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

    /// <summary>
    /// 使用最近最少使用（LRU）算法的缓存
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class LRUCache<K, V>
    {
        private readonly Dictionary<K, int> _Dic = new Dictionary<K, int>();
        private readonly Node[] _Nodes;
        private int _First;
        private int _Last;

        public LRUCache(int capacity)
        {
            Assert.IsTrue(capacity > 0, "容量需要大于0");
            _Nodes = new Node[capacity];
            _Last = 0;
            _First = capacity - 1;
            _Nodes[_Last].Next = _Nodes[_First].Pre = -1;
            for (var i = 1; i < capacity; i++)
            {
                _Nodes[i - 1].Pre = i;
                _Nodes[i].Next = i - 1;
            }
        }

        public bool Contains(K key)
        {
            return _Dic.TryGetValue(key, out _);
        }

        public V this[K key]
        {
            get => Get(key);
            set => Add(key,value);
        }

        public V Get(K key)
        {
            if (!_Dic.TryGetValue(key, out var result)) 
                throw new KeyNotFoundException(); // 缓存中没有key，抛出异常

            if (_Nodes[result].Pre == -1) return _Nodes[result].Val; // 节点是头部，直接返回值

            MoveToFirst(result); // 将节点移动到头部并返回值
            return _Nodes[_First].Val;
        }

        public bool TryGet(K key, out V value)
        {
            if (!_Dic.TryGetValue(key, out var result))
            {
                value = default;
                return false;
            }

            if (_Nodes[result].Pre == -1)
            {
                value = _Nodes[result].Val;
                return true; // 节点是头部，直接返回值
            }

            MoveToFirst(result); // 将节点移动到头部并返回值
            value = _Nodes[_First].Val;
            return true;
        }

        public void Add(K key, V value)
        {
            if (_Dic.TryGetValue(key, out var node)) // 如果已存在key，则将节点值重新设置并放置在头部
            {
                _Nodes[node].Val = value;
                MoveToFirst(node);
            }
            else
            {
                var temp = _Last;
                if (_Nodes[temp].Key != null)
                    _Dic.Remove(_Nodes[temp].Key);
                // 若前方为-1则说明缓冲区大小为1，这里不为1
                // 所以将尾部节点的数据抛弃，重新赋值后放置在首部
                // 为1则跳过这些操作
                if (_Nodes[temp].Pre != -1)
                {
                    _Last = _Nodes[temp].Pre;
                    _Nodes[_Nodes[temp].Pre].Next = -1;
                    _Nodes[temp].Pre = -1;
                    _Nodes[temp].Next = _First;
                    _Nodes[_First].Pre = temp;
                    _First = temp;
                }

                _Nodes[temp].Key = key;
                _Nodes[temp].Val = value;
                _Dic.Add(key, temp);
            }
        }

        private void MoveToFirst(int node)
        {
            if (node == _First) return;
            // 将节点移除出当前位置并恢复其附近的连接
            _Nodes[_Nodes[node].Pre].Next = _Nodes[node].Next; // 将节点前方的后方设为节点的后方

            if (_Nodes[node].Next != -1) // 不是尾部
                _Nodes[_Nodes[node].Next].Pre = _Nodes[node].Pre; // 则将后方的前方设为节点前方
            else
                _Last = _Nodes[node].Pre; // 如果是尾部，则将尾部设为节点的前方

            // 将当前节点放置在链表头部
            _Nodes[_First].Pre = node;
            _Nodes[node].Next = _First;
            _Nodes[node].Pre = -1;
            _First = node;
        }

        private struct Node
        {
            public int Pre;
            public int Next;
            public K Key;
            public V Val;
        }
    }

    /// <summary>
    /// 在检查条件发生变化时执行所给定的函数
    /// </summary>
    public class SwitchTrigger
    {
        public Action<bool> Trigger { get; set; }
        private bool _Pre;

        public SwitchTrigger(Action<bool> act, bool startValue = false)
        {
            Trigger = act;
            _Pre = startValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns>是否发生了切换事件</returns>
        public bool Check(bool condition)
        {
            var result = false;
            if (_Pre && !condition)
            {
                _Pre = false;
                Trigger?.Invoke(false);
                result = true;
            }
            else if (!_Pre && condition)
            {
                _Pre = true;
                Trigger?.Invoke(true);
                result = true;
            }

            return result;
        }

        public static implicit operator bool(SwitchTrigger t)
        {
            return t._Pre;
        }
    }

    /// <summary>
    ///     粘滞输入，通过设置按键的生存时间来延长输入
    /// </summary>
    public class StickyInputDic
    {
        private readonly Dictionary<string, InputUnit> _Inputs = new Dictionary<string, InputUnit>();

        public void AddFloat(string key, float lifeTime, float floatDef = 0)
        {
            _Inputs.Add(key, new InputUnit(lifeTime, floatDef, 0, false));
        }

        public void AddInt(string key, float lifeTime, int intDef = 0)
        {
            _Inputs.Add(key, new InputUnit(lifeTime, 0, intDef, false));
        }

        public void AddBool(string key, float lifeTime, bool boolDef = false)
        {
            _Inputs.Add(key, new InputUnit(lifeTime, 0, 0, boolDef));
        }

        public void SetFloat(string key, float value, bool force = false)
        {
            var unit = _Inputs[key];
            if (force || value != unit.FloatDef)
            {
                unit.Float = value;
                unit.LifeTimeCount = Time.time;
            }

            _Inputs[key] = unit;
        }

        public void SetInt(string key, int value, bool force = false)
        {
            var unit = _Inputs[key];
            if (force || value != unit.IntegerDef)
            {
                unit.Integer = value;
                unit.LifeTimeCount = Time.time;
            }

            _Inputs[key] = unit;
        }

        public void SetBool(string key, bool value, bool force = false)
        {
            var unit = _Inputs[key];
            if (force || value != unit.BooleanDef)
            {
                unit.Boolean = value;
                unit.LifeTimeCount = Time.time;
            }

            _Inputs[key] = unit;
        }

        public float GetFloat(string key)
        {
            return _Inputs[key].Float;
        }

        public bool GetBool(string key)
        {
            return _Inputs[key].Boolean;
        }

        public int GetInt(string key)
        {
            return _Inputs[key].Integer;
        }

        public void FixedUpdate()
        {
            foreach (var inputsKey in _Inputs.Keys.ToArray())
            {
                var unit = _Inputs[inputsKey];
                if (Time.time - unit.LifeTimeCount > unit.LifeTime)
                {
                    unit.Float = unit.FloatDef;
                    unit.Boolean = unit.BooleanDef;
                    unit.Integer = unit.IntegerDef;
                    unit.LifeTimeCount = Time.time;
                }

                _Inputs[inputsKey] = unit;
            }
        }

        private struct InputUnit
        {
            public float Float;
            public int Integer;
            public bool Boolean;

            public readonly float FloatDef;
            public readonly int IntegerDef;
            public readonly bool BooleanDef;
            public readonly float LifeTime;
            public float LifeTimeCount;

            public InputUnit(float lifeTime, float floatDef, int integerDef, bool booleanDef)
            {
                Float = 0;
                Integer = 0;
                Boolean = false;
                FloatDef = floatDef;
                IntegerDef = integerDef;
                BooleanDef = booleanDef;
                LifeTime = lifeTime;
                LifeTimeCount = 0;
            }
        }
    }
}

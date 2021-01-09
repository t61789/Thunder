using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace Framework
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
    /// 管道队列，索引从尾部开始，空间不足时自动抛弃首部元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PipelineQueue<T> : IEnumerable<T>
    {
        private T[] _Buffer;
        private int _Head;
        private int _Tail;
        private int _Version;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity">管道容量</param>
        public PipelineQueue(int capacity)
        {
            if (capacity <= 0)
                throw new Exception($"管道容量不正确 {capacity}");
            _Buffer = new T[capacity];
        }

        /// <summary>
        ///     缓冲区的大小
        /// </summary>
        public int Capacity
        {
            get => _Buffer.Length;

            set => SetCapacity(value);
        }

        /// <summary>
        ///     缓冲区中元素的数量
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns> 
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();
                return _Buffer[LimitPointer(_Tail + index)];
            }

            set
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();
                _Buffer[LimitPointer(_Tail + index)] = value;
            }
        }

        /// <summary>
        ///     在环形缓冲区的队尾添加元素
        /// </summary>
        /// <param name="value"></param>
        /// <returns>当缓冲区已满时返回被覆盖的元素</returns>
        public T Enqueue(T value)
        {
            if (Count == 0)
            {
                _Buffer[_Tail] = value;
                Count++;
                _Version++;
                return default;
            }

            var tailLimited = LimitPointer(_Tail - 1);
            T result = default;
            if (tailLimited == _Head)
            {
                result = _Buffer[tailLimited];
                _Head = LimitPointer(_Head - 1);
                Count--;
            }

            _Tail = tailLimited;
            _Buffer[_Tail] = value;
            Count++;
            _Version++;
            return result;
        }

        /// <summary>
        ///     移除队首的元素
        /// </summary>
        /// <returns>被移除的元素</returns>
        public T Dequeue()
        {
            if (Count == 0) return default;
            T result = _Buffer[_Head];
            _Buffer[_Head] = default;
            if (Count != 1)
                _Head = LimitPointer(_Head - 1);

            Count--;
            _Version++;
            return result;
        }

        /// <summary>
        ///     清空缓存
        /// </summary>
        public void Clear()
        {
            while (Count != 0)
            {
                _Buffer[_Head] = default;
                if (Count != 1)
                    _Head = LimitPointer(_Head + 1);
                Count--;
            }

            _Version++;
        }

        /// <summary>
        /// 设置管道的容量，小于容量的元素会被抛弃
        /// </summary>
        /// <param name="capacity"></param>
        public void SetCapacity(int capacity)
        {
            if (capacity <= 0)
                throw new Exception($"管道容量不正确 {capacity}");

            var newBuffer = new T[capacity];
            if (_Tail < _Head)
            {
                var tempLength = Math.Min(_Buffer.Length - _Tail, capacity);
                Array.Copy(
                    _Buffer,
                    _Head,
                    newBuffer,
                    0,
                    tempLength);
                capacity -= tempLength;

                if (capacity != 0)
                {
                    Array.Copy(
                        _Buffer,
                        0,
                        newBuffer,
                        tempLength,
                        Math.Min(capacity, _Tail + 1));
                }

                _Head = 0;
                _Tail = Count - 1;
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

        private int LimitPointer(int p)
        {
            if (p >= 0) return p % _Buffer.Length;
            p %= _Buffer.Length;
            p += _Buffer.Length;
            return p;
        }

        private IEnumerator<T> Enumrator()
        {
            var startVersion = _Version;
            for (int i = 0; i < Count; i++)
            {
                if (_Version != startVersion)
                    throw new InvalidOperationException();
                yield return this[i];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumrator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
            set => Add(key, value);
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

    public class DisposableDictionary
    {
        private readonly Dictionary<string, bool> _BoolDic = new Dictionary<string, bool>();
        private readonly Dictionary<string, float> _FloatDic = new Dictionary<string, float>();
        private readonly Dictionary<string, int> _IntDic = new Dictionary<string, int>();
        private readonly Dictionary<string, Vector3> _VectorDic = new Dictionary<string, Vector3>();

        public int GetInt(string key, int defaultValue = 0)
        {
            try
            {
                var result = _IntDic[key];
                _IntDic[key] = defaultValue;
                return result;
            }
            catch (Exception)
            {
                _IntDic.Add(key, defaultValue);
                return defaultValue;
            }
        }

        public void SetInt(string key, int value)
        {
            try
            {
                _IntDic[key] = value;
            }
            catch (Exception)
            {
                _IntDic.Add(key, value);
            }
        }

        public float GetFloat(string key, float defaultValue = 0)
        {
            try
            {
                var result = _FloatDic[key];
                _FloatDic[key] = defaultValue;
                return result;
            }
            catch (Exception)
            {
                _FloatDic.Add(key, defaultValue);
                return defaultValue;
            }
        }

        public void SetFloat(string key, float value)
        {
            try
            {
                _FloatDic[key] = value;
            }
            catch (Exception)
            {
                _FloatDic.Add(key, value);
            }
        }

        public Vector3 GetVector(string key, Vector3 defaultValue = default)
        {
            try
            {
                var result = _VectorDic[key];
                _VectorDic[key] = defaultValue;
                return result;
            }
            catch (Exception)
            {
                _VectorDic.Add(key, defaultValue);
                return defaultValue;
            }
        }

        public void SetVector(string key, Vector3 value)
        {
            try
            {
                _VectorDic[key] = value;
            }
            catch (Exception)
            {
                _VectorDic.Add(key, value);
            }
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            try
            {
                var result = _BoolDic[key];
                _BoolDic[key] = defaultValue;
                return result;
            }
            catch (Exception)
            {
                _BoolDic.Add(key, defaultValue);
                return defaultValue;
            }
        }

        public void SetBool(string key, bool value)
        {
            try
            {
                _BoolDic[key] = value;
            }
            catch (Exception)
            {
                _BoolDic.Add(key, value);
            }
        }
    }

    [JsonObject]
    public struct SerializableVector3
    {
        public float[] Pos;
        [JsonIgnore] public Vector3 Inner;

        public SerializableVector3(float x, float y, float z)
        {
            Pos = new float[3];
            Pos[0] = x;
            Pos[1] = y;
            Pos[2] = z;
            Inner = Vector3.zero;
        }

        public override string ToString()
        {
            if (Inner.Equals(Vector3.zero))
                Inner = Pos == null ? Vector3.zero : new Vector3(Pos[0], Pos[1], Pos[2]);

            return Inner.ToString();
        }

        public static implicit operator Vector3(SerializableVector3 s)
        {
            if (s.Inner.Equals(Vector3.zero))
                s.Inner = s.Pos == null ? Vector3.zero : new Vector3(s.Pos[0], s.Pos[1], s.Pos[2]);

            return s.Inner;
        }

        public static implicit operator SerializableVector3(Vector3 v)
        {
            return new SerializableVector3(v.x, v.y, v.z);
        }
    }

    [XmlRoot("dictionary")]
    public struct SerializableDictionary<K, V> : IXmlSerializable
    {
        private Dictionary<K, V> _Dic;
        private XmlSerializer _KSerializer;
        private XmlSerializer _VSerializer;

        public SerializableDictionary(Dictionary<K, V> dic)
        {
            this._Dic = dic;
            _KSerializer = new XmlSerializer(typeof(K));
            _VSerializer = new XmlSerializer(typeof(V));
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            _Dic = new Dictionary<K, V>();
            if (reader.IsEmptyElement) return;
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
        }

        public static implicit operator SerializableDictionary<K, V>(Dictionary<K, V> dic)
        {
            return new SerializableDictionary<K, V>(dic);
        }
    }

    /// <summary>
    /// 柏林噪声
    /// </summary>
    public struct PerlinNoise
    {
        private const float RANDOM_RANGE = 100;

        public Vector2 StartPos;
        public Vector2 Dir;
        public float Smooth;

        /// <summary>
        ///     指定的起始位置和方向
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="dir">方向，需为归一化</param>
        /// <param name="smooth">平滑度，绝对值越小越平滑</param>
        public PerlinNoise(Vector2 startPos, Vector2 dir, float smooth)
        {
            StartPos = startPos;
            Dir = dir;
            Smooth = smooth;
        }

        /// <summary>
        ///     随机的起始位置和方向
        /// </summary>
        /// <param name="smooth">平滑度，绝对值越小越平滑</param>
        public PerlinNoise(float smooth)
        {
            StartPos = Tools.RandomVectorInCircle(RANDOM_RANGE);
            Dir = Tools.RandomVectorInCircle(1).normalized;
            Smooth = smooth;
        }

        /// <summary>
        ///     获取下一个采样点
        /// </summary>
        /// <returns>噪声值，介于[0,1]内</returns>
        public float Next()
        {
            StartPos += Smooth * Dir;
            return Mathf.PerlinNoise(StartPos.x, StartPos.y);
        }
    }

    public class BuffData
    {
        // 用于实现Buff机制的类，采用树状结构储存
        // 每个节点有一个父节点和多个子节点
        // 每次子节点更新值后会递归更改所有父节点的值
        private readonly List<Unit> _Child = new List<Unit>();
        private readonly List<BuffData> _Parent = new List<BuffData>();
        private readonly StringBuilder _Sb = new StringBuilder();

        public float BaseData
        {
            get => _BaseData;
            set
            {
                if (_BaseData == value) return;
                _BaseData = value;
                ReCalculateData();
            }
        }
        private float _BaseData;

        private float _CurData;

        public BuffData(float baseData)
        {
            BaseData = baseData;
        }

        /// <summary>
        /// 添加一个buff，不可重复
        /// </summary>
        /// <param name="newBuff"></param>
        /// <param name="op"></param>
        /// <param name="priority">优先级越小的buff越先计算，相同则先来先计算</param>
        public void AddBuff(BuffData newBuff, Operator op, int priority = 0)
        {
            Assert.IsFalse(HasChild(newBuff), "不可重复添加相同的buff");
            _Child.Add(new Unit(op, priority, newBuff));
            for (var i = _Child.Count - 1; i > 0; i--)
                if (_Child[i - 1].Priority > _Child[i].Priority)
                {
                    var temp = _Child[i - 1];
                    _Child[i - 1] = _Child[i];
                    _Child[i] = temp;
                }
                else
                    break;
            newBuff._Parent.Add(this);

            ReCalculateData();
        }

        /// <summary>
        /// 移除一个buff
        /// </summary>
        /// <param name="buff"></param>
        public void RemoveBuff(BuffData buff)
        {
            int index = _Child.FindIndex(x => x.BuffData == buff);
            Assert.IsFalse(index == -1, "不存在buff");
            _Child[index].BuffData._Parent.Remove(this);
            _Child.RemoveAt(index);

            ReCalculateData();
        }

        /// <summary>
        /// 清除这个节点的所有引用/被引用关系
        /// </summary>
        public void Destroy()
        {
            while (_Parent.Count != 0)
                _Parent[0].RemoveBuff(this);
            while (_Child.Count != 0)
                RemoveBuff(_Child[0].BuffData);
        }

        /// <summary>
        /// 获得当前所有buff的字符串表示
        /// </summary>
        /// <returns></returns>
        public string GetAllBuff()
        {
            const char d = ',';
            const char l = '(';
            const char r = ')';
            _Sb.Clear();
            foreach (var unit in _Child)
            {
                _Sb.Append(l);
                _Sb.Append(unit.BuffData._CurData);
                _Sb.Append(d);
                _Sb.Append(unit.Priority);
                _Sb.Append(d);
                _Sb.Append(unit.Op);
                _Sb.Append(r);
            }

            return _Sb.ToString();
        }

        /// <summary>
        /// 是否被指定对象引用
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool HasParent(BuffData parent)
        {
            return _Parent.FindIndex(x => x == parent) != -1;
        }

        /// <summary>
        /// 是否有在引用对象
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public bool HasChild(BuffData child)
        {
            return _Child.FindIndex(x => x.BuffData == child) != -1;
        }

        private void ReCalculateData()
        {
            // 递归修改所有父节点的当前值
            _CurData = BaseData;
            foreach (var t in _Child)
                switch (t.Op)
                {
                    case Operator.Add:
                        _CurData += t.BuffData;
                        break;
                    case Operator.Div:
                        _CurData /= t.BuffData;
                        break;
                    case Operator.Sub:
                        _CurData -= t.BuffData;
                        break;
                    case Operator.Mul:
                        _CurData *= t.BuffData;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            foreach (var unit in _Parent)
                unit.ReCalculateData();
        }

        public override string ToString()
        {
            return _CurData.ToString(CultureInfo.InvariantCulture);
        }

        public static implicit operator float(BuffData buff)
        {
            return buff._CurData;
        }

        public struct Unit
        {
            public Operator Op;
            public int Priority;
            public BuffData BuffData;

            public Unit(Operator op, int priority, BuffData buffData)
            {
                Op = op;
                Priority = priority;
                BuffData = buffData;
            }
        }
    }

    public enum Operator
    {
        Add,
        Sub,
        Mul,
        Div
    }

    public class ConsoleWindow
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private TextWriter _OldOutput;

        public void Initialize()
        {
            if (!AttachConsole(0x0ffffffff)) AllocConsole();

            _OldOutput = Console.Out;

            try
            {
                var stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                var safeFileHandle = new SafeFileHandle(stdHandle, true);
                var fileStream = new FileStream(safeFileHandle, FileAccess.Write);
                var encoding = Encoding.ASCII;
                var standardOutput = new StreamWriter(fileStream, encoding) { AutoFlush = true };
                Console.SetOut(standardOutput);

                Application.logMessageReceived += Say;
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't redirect output: " + e.Message);
            }
        }

        public void Say(string condition, string stackTrace, LogType type)
        {
            var sb = new StringBuilder();
            sb.Append("UnityEngine:>");
            sb.Append(type.ToString());
            sb.Append("\n");
            sb.Append(condition);
            sb.Append("\n");
            sb.Append(stackTrace);
            sb.Append("\n");

            Console.WriteLine(sb.ToString());
        }

        public void Shutdown()
        {
            Console.SetOut(_OldOutput);
            FreeConsole();
        }

        public void SetTitle(string strName)
        {
            SetConsoleTitle(strName);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleTitle(string lpConsoleTitle);
    }

    public class VectorComparer : Comparer<Vector3>
    {
        public new static readonly VectorComparer Default;
        public bool Des;

        static VectorComparer()
        {
            Default = new VectorComparer(false);
        }

        public VectorComparer(bool des)
        {
            Des = des;
        }

        public override int Compare(Vector3 x, Vector3 y)
        {
            int result;
            if (x.x > y.x)
                result = 1;
            else if (x.x < y.x)
                result = -1;
            else if (x.y > y.y)
                result = 1;
            else if (x.y < y.y)
                result = -1;
            else
                result = 0;
            if (Des) result = -result;
            return result;
        }
    }

    public abstract class Counter
    {
        protected float TimeCountStart;
        public float TimeLimit { protected set; get; }

        protected Counter(float timeLimit)
        {
            TimeLimit = timeLimit;
            TimeCountStart = Time.time;
        }

        /// <summary>
        ///     当前计时与目标时间的插值，经过clamp处理
        /// </summary>
        public float Interpolant =>
            Tools.InLerp(0, TimeLimit, TimeCount);

        /// <summary>
        ///     当前计时与目标时间的插值，未经clamp处理
        /// </summary>
        public float InterpolantUc =>
            Tools.InLerpUc(0, TimeLimit, TimeCount);

        /// <summary>
        ///     当前已经记录了多少时间
        /// </summary>
        public abstract float TimeCount { get; }

        /// <summary>
        ///     指示是否已经完成计时
        /// </summary>
        public abstract bool Completed { get; }

        /// <summary>
        /// 重新计数
        /// </summary>
        /// <param name="timeLimit">新的计数时限，若为-1则不做改动</param>
        public abstract void Recount(float timeLimit = -1);

        /// <summary>
        /// 根据指定的factor设置当前计数器的计时起点
        /// </summary>
        public abstract void SetCountValue(float factor);
    }

    public class SimpleCounter : Counter
    {
        public SimpleCounter(float timeLimit, bool countAtStart = true) : base(timeLimit)
        {
            if (countAtStart) return;
            TimeCountStart -= TimeLimit;
        }

        public override float TimeCount =>
            Time.time - TimeCountStart;

        public override bool Completed => Time.time >= TimeCountStart + TimeLimit;

        public override void Recount(float timeLimit = -1)
        {
            TimeLimit = timeLimit == -1 ? TimeLimit : timeLimit;
            TimeCountStart = Time.time;
        }

        public override void SetCountValue(float factor)
        {
            TimeCountStart = Time.time - factor * TimeLimit;
        }

        /// <summary>
        ///     立即完成计时
        /// </summary>
        /// <returns></returns>
        public SimpleCounter Complete()
        {
            TimeCountStart = Time.time - TimeLimit;
            return this;
        }
    }

    public class SimpleCounterQueue
    {
        public event Action<int> OnStageCompleted;

        private readonly float[] _TimeLimitQueue;

        public SimpleCounter Counter { get; }
        public int CurStage { private set; get; }

        public SimpleCounterQueue(MonoBehaviour host, SimpleCounter counter, float[] timeLimitQueue)
        {
            Counter = counter;
            _TimeLimitQueue = timeLimitQueue;
            host.StartCoroutine(Count());
        }

        public void Play(int stage = 0)
        {
            CurStage = stage;
            Counter.Recount(_TimeLimitQueue[CurStage]);
        }

        public void Stop()
        {
            CurStage = _TimeLimitQueue.Length;
        }

        private IEnumerator Count()
        {
            while (true)
            {
                yield return null;
                if (CurStage == _TimeLimitQueue.Length || !Counter.Completed) continue;
                OnStageCompleted?.Invoke(CurStage);
                CurStage++;
                if (CurStage == _TimeLimitQueue.Length) continue;
                Counter.Recount(_TimeLimitQueue[CurStage]);
            }
        }
    }

    /// <summary>
    /// 半自动计时器，使用提供的Update和FixedUpdate实现自动执行回调函数，而非协程<br/>
    /// 用法大体与自动计时器相同，但对性能更友好，适用于大量简单物体的计时
    /// </summary>
    public class SemiAutoCounter : Counter
    {
        private float _CountPauseSave;
        private bool _HasExcutedCompleteCallBack;
        private Action _OnCompleteCallBack;
        private bool _Running = true;

        /// <summary>
        /// </summary>
        /// <param name="timeLimit"></param>
        public SemiAutoCounter(float timeLimit) : base(timeLimit)
        {
            TimeCountStart = Time.time;
        }

        public override float TimeCount
        {
            get
            {
                if (!_Running)
                    return _CountPauseSave;
                return Time.time - TimeCountStart;
            }
        }

        public override bool Completed => _HasExcutedCompleteCallBack;

        public override void Recount(float timeLimit = -1)
        {
            TimeLimit = timeLimit == -1 ? TimeLimit : timeLimit;
            TimeCountStart = Time.time;
            _HasExcutedCompleteCallBack = false;
            _CountPauseSave = 0;
        }

        /// <summary>
        ///     立即完成计时
        /// </summary>
        /// <param name="callback">是否调用完成回调函数</param>
        /// <returns></returns>
        public SemiAutoCounter Complete(bool callback = true)
        {
            TimeCountStart = Time.time - TimeLimit;
            if (callback)
                _OnCompleteCallBack?.Invoke();
            _HasExcutedCompleteCallBack = true;
            _CountPauseSave = TimeLimit;
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
            TimeCountStart = Time.time - _CountPauseSave;
            return this;
        }

        /// <summary>
        ///     暂停自动计时
        /// </summary>
        /// <returns></returns>
        public SemiAutoCounter Pause()
        {
            _Running = false;
            _CountPauseSave = Time.time - TimeCountStart;
            return this;
        }

        public void Update()
        {
            if (_HasExcutedCompleteCallBack || !_Running || Time.time <= TimeCountStart + TimeLimit) return;
            _HasExcutedCompleteCallBack = true;
            _OnCompleteCallBack?.Invoke();
        }

        public void FixedUpdate()
        {
            if (_HasExcutedCompleteCallBack || !_Running || Time.time <= TimeCountStart + TimeLimit) return;
            _HasExcutedCompleteCallBack = true;
            _OnCompleteCallBack?.Invoke();
        }

        public override void SetCountValue(float factor)
        {
            if (_Running)
                TimeCountStart = Time.time - factor * TimeLimit;
            else
                _CountPauseSave = TimeLimit * factor;

            if (factor <= 0)
                _HasExcutedCompleteCallBack = false;
        }
    }

    /// <summary>
    /// 用于将多个半自动计时器归入一个集合中进行更新
    /// </summary>
    public class SemiAutoCounterHub
    {
        private readonly List<SemiAutoCounter> _Counters;

        public SemiAutoCounterHub(params SemiAutoCounter[] counters)
        {
            _Counters = new List<SemiAutoCounter>(counters);
        }

        public void Update()
        {
            foreach (var t in _Counters)
                t.Update();
        }

        public void FixedUpdate()
        {
            foreach (var t in _Counters)
                t.FixedUpdate();
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

    /// <summary>
    /// 自动计时器，采用协程
    /// </summary>
    public class AutoCounter : Counter
    {
        private float _CountPauseSave;
        private bool _HasExcutedCompleteCallBack;
        private Action _OnCompleteCallBack;
        private bool _Running = true;
        private Coroutine _CountCoroutine;

        /// <summary>
        /// </summary>
        /// <param name="parent">协程附着对象</param>
        /// <param name="timeLimit"></param>
        public AutoCounter(MonoBehaviour parent, float timeLimit) : base(timeLimit)
        {
            TimeCountStart = Time.time;
            _CountCoroutine = parent.StartCoroutine(Count());
        }

        public override float TimeCount
        {
            get
            {
                if (!_Running)
                    return _CountPauseSave;
                return Time.time - TimeCountStart;
            }
        }

        public override bool Completed => _HasExcutedCompleteCallBack;

        public override void Recount(float timeLimit = -1)
        {
            TimeLimit = timeLimit == -1 ? TimeLimit : timeLimit;
            TimeCountStart = Time.time;
            _HasExcutedCompleteCallBack = false;
            _CountPauseSave = 0;
        }

        public override void SetCountValue(float factor)
        {
            if (_Running)
                TimeCountStart = Time.time - factor * TimeLimit;
            else
                _CountPauseSave = TimeLimit * factor;

            if (factor <= 0)
                _HasExcutedCompleteCallBack = false;
        }

        public AutoCounter Complete(bool callback = true)
        {
            TimeCountStart = Time.time - TimeLimit;
            if (callback)
                _OnCompleteCallBack?.Invoke();
            _HasExcutedCompleteCallBack = true;
            _CountPauseSave = TimeLimit;
            return this;
        }

        /// <summary>
        ///     启用自动计时后调用该回调函数
        /// </summary>
        /// <param name="callBack">回调函数</param>
        /// <returns></returns>
        public AutoCounter OnComplete(Action callBack)
        {
            _OnCompleteCallBack = callBack;
            return this;
        }

        /// <summary>
        ///     恢复自动计时，如果当前正在自动计时，则重置计数
        /// </summary>
        /// <returns></returns>
        public AutoCounter Resume()
        {
            _Running = true;
            TimeCountStart = Time.time - _CountPauseSave;
            return this;
        }

        /// <summary>
        ///     暂停自动计时
        /// </summary>
        /// <returns></returns>
        public AutoCounter Pause()
        {
            _Running = false;
            _CountPauseSave = Time.time - TimeCountStart;
            return this;
        }

        public void Dispose()
        {
            _CountCoroutine = null;
        }

        private IEnumerator Count()
        {
            while (_CountCoroutine != null)
            {
                if (!_HasExcutedCompleteCallBack && _Running && Time.time >= TimeCountStart + TimeLimit)
                {
                    _HasExcutedCompleteCallBack = true;
                    _OnCompleteCallBack?.Invoke();
                }

                yield return null;
            }
        }
    }

    /// <summary>
    /// 表示一个自然数的范围
    /// </summary>
    [Serializable]
    public class Range
    {
        public float Min;
        public float Max;

        public Range(float min, float max)
        {
            CheckParam(min, max);
            Min = min;
            Max = max;
        }

        public Range((float min, float max) range)
        {
            CheckParam(range.min,range.max);
            Min = range.min;
            Max = range.max;
        }

        private static void CheckParam(float min, float max)
        {
            if(min>max)
                throw new Exception($"最小值 {min} 不得大于最大值 {max}");
        }

        public static implicit operator Range((float min, float max) range)
        {
            return new Range(range);
        }
    }
}

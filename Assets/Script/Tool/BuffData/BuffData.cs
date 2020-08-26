using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Thunder.Tool.BuffData
{
    [Serializable]
    public class BuffData
    {
        // 用于实现Buff机制的类，采用树状结构储存
        // 每个节点有一个父节点和多个子节点
        // 每次子节点更新值后会递归更改所有父节点的值
        public enum Operator
        {
            Add,
            Sub,
            Mul,
            Div
        }

        public struct Unit
        {
            public string Name;
            public Operator Op;
            public float Priority;
            public BuffData BuffData;

            public Unit(string name, Operator op, float priority, BuffData buffData)
            {
                Name = name;
                Op = op;
                Priority = priority;
                BuffData = buffData;
            }
        }

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
        [SerializeField]
        private float _BaseData;

        public float CurData
        {
            get
            {
                if (_CurDataChanged) return _CurData;
                CurData = BaseData;
                _CurDataChanged = true;
                return _CurData;
            }
            set
            {
                if (_CurData == value) return;
                _CurData = value;
                Parent?.ReCalculateData();
            }
        }
        private float _CurData;
        private bool _CurDataChanged;

        [HideInInspector]
        public BuffData Parent { get; set; }

        [HideInInspector]
        public UnityEvent DataChanged;

        private readonly List<Unit> _Child = new List<Unit>();

        public BuffData(float baseData)
        {
            BaseData = baseData;
        }

        public void ReCalculateData()
        {
            // 递归修改所有父节点的当前值
            float newData = BaseData;
            foreach (var t in _Child)
            {
                switch (t.Op)
                {
                    case Operator.Add:
                        newData += t.BuffData.CurData;
                        break;
                    case Operator.Div:
                        newData /= t.BuffData.CurData;
                        break;
                    case Operator.Sub:
                        newData -= t.BuffData.CurData;
                        break;
                    case Operator.Mul:
                        newData *= t.BuffData.CurData;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            CurData = newData;
        }

        public void AddBuff(BuffData newBuff, string name, Operator op, float priority)
        {
            newBuff.Parent = this;
            _Child.Add(new Unit(name, op, priority, newBuff));
            for (int i = _Child.Count - 1; i > 0; i--)
            {
                if (_Child[i - 1].Priority > _Child[i].Priority)
                {
                    Unit temp = _Child[i - 1];
                    _Child[i - 1] = _Child[i];
                    _Child[i] = temp;
                }
                else break;
            }

            ReCalculateData();

            DataChanged?.Invoke();
        }

        public void RemoveBuff(BuffData buf)
        {
            for (int i = 0; i < _Child.Count; i++)
            {
                if (_Child[i].BuffData != buf) continue;
                _Child[i].BuffData.Parent = null;
                _Child.RemoveAt(i);
                return;
            }
            ReCalculateData();

            DataChanged?.Invoke();
        }

        public void RemoveBuff(string name)
        {
            for (int i = 0; i < _Child.Count; i++)
            {
                if (_Child[i].Name != name) continue;
                _Child[i].BuffData.Parent = null;
                _Child.RemoveAt(i);
                break;
            }
            ReCalculateData();

            DataChanged?.Invoke();
        }

        public void Destroy()
        {
            Parent?.RemoveBuff(this);
            foreach (var nBuffData in _Child)
                nBuffData.BuffData.Parent = null;
        }

        public static implicit operator float(BuffData buff)
        {
            return buff.CurData;
        }

        public static implicit operator BuffData(float buff)
        {
            return new BuffData(buff);
        }

        public static float operator +(BuffData v1, BuffData v2)
        {
            return v1.CurData + v2.CurData;
        }

        public static float operator +(float v1, BuffData v2)
        {
            return v1 + v2.CurData;
        }

        public static float operator +(BuffData v1, float v2)
        {
            return v1.CurData + v2;
        }

        public static float operator -(BuffData v1, BuffData v2)
        {
            return v1.CurData + v2.CurData;
        }

        public static float operator -(float v1, BuffData v2)
        {
            return v1 - v2.CurData;
        }

        public static float operator -(BuffData v1, float v2)
        {
            return v1.CurData - v2;
        }

        public static float operator *(BuffData v1, BuffData v2)
        {
            return v1.CurData * v2.CurData;
        }

        public static float operator *(float v1, BuffData v2)
        {
            return v1 * v2.CurData;
        }

        public static float operator *(BuffData v1, float v2)
        {
            return v1.CurData * v2;
        }

        public static float operator /(BuffData v1, BuffData v2)
        {
            return v1.CurData / v2.CurData;
        }

        public static float operator /(float v1, BuffData v2)
        {
            return v1 / v2.CurData;
        }

        public static float operator /(BuffData v1, float v2)
        {
            return v1.CurData / v2;
        }

        public static Vector3 operator *(Vector3 v1, BuffData v2)
        {
            return v1 * v2.CurData;
        }

        public static Vector3 operator *(BuffData v1, Vector3 v2)
        {
            return v1.CurData * v2;
        }

        private readonly StringBuilder _Sb = new StringBuilder();
        public override string ToString()
        {
            return CurData.ToString();
        }

        public string GetAllBuff()
        {
            const char d = ',';
            const char l = '(';
            const char r = ')';
            _Sb.Clear();
            foreach (var unit in _Child)
            {
                _Sb.Append(l);
                _Sb.Append(unit.Name);
                _Sb.Append(d);
                _Sb.Append(unit.BuffData.CurData);
                _Sb.Append(d);
                _Sb.Append(unit.Priority);
                _Sb.Append(d);
                _Sb.Append(unit.Op);
                _Sb.Append(r);
            }

            return _Sb.ToString();
        }
    }

}
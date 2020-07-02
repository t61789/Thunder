using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tool
{
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

            public Unit(string name,Operator op, float priority, BuffData buffData)
            {
                Name = name;
                Op = op;
                Priority = priority;
                BuffData = buffData;
            }
        }

        public float BaseData
        {
            get => _baseData;
            set
            {
                if (_baseData == value) return;
                _baseData = value;
                ReCalculateData();
            }
        }
        private float _baseData;

        public float CurData
        {
            get => _curData;
            set
            {
                if (_curData == value) return;
                _curData = value;
                Parent?.ReCalculateData();
            }
        }
        private float _curData;

        public BuffData Parent { get; set; }

        private readonly List<Unit> _child = new List<Unit>();

        public BuffData(float baseData)
        {
            BaseData = baseData;
        }

        public void ReCalculateData()
        {
            // 递归修改所有父节点的当前值
            float newData = BaseData;
            foreach (var t in _child)
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

        public void AddBuff(BuffData newBuff,string name, Operator op, float priority)
        {
            newBuff.Parent = this;
            _child.Add(new Unit(name,op, priority, newBuff));
            for (int i = _child.Count - 1; i > 0; i--)
            {
                if (_child[i - 1].Priority > _child[i].Priority)
                {
                    Unit temp = _child[i - 1];
                    _child[i - 1] = _child[i];
                    _child[i] = temp;
                }
                else break;
            }

            ReCalculateData();
        }

        public void RemoveBuff(BuffData buf)
        {
            for (int i = 0; i < _child.Count; i++)
            {
                if (_child[i].BuffData != buf) continue;
                _child[i].BuffData.Parent = null;
                _child.RemoveAt(i);
                return;
            }
            ReCalculateData();
        }

        public void RemoveBuff(string name)
        {
            for (int i = 0; i < _child.Count; i++)
            {
                if (_child[i].Name != name) continue;
                _child[i].BuffData.Parent = null;
                _child.RemoveAt(i);
                return;
            }
            ReCalculateData();
        }

        public void Destroy()
        {
            Parent?.RemoveBuff(this);
            foreach (var nBuffData in _child)
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
            foreach (var unit in _child)
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Thunder.Tool.BuffData
{
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
        public void AddBuff(BuffData newBuff, Operator op, int priority=0)
        {
            Assert.IsFalse(HasChild(newBuff),"不可重复添加相同的buff");
            _Child.Add(new Unit( op, priority, newBuff));
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
            int index = _Child.FindIndex(x=>x.BuffData==buff);
            Assert.IsFalse(index==-1,"不存在buff");
            _Child[index].BuffData._Parent.Remove(this);
            _Child.RemoveAt(index);

            ReCalculateData();
        }
 
        /// <summary>
        /// 清除这个节点的所有引用/被引用关系
        /// </summary>
        public void Destroy()
        {
            while(_Parent.Count!=0)
                _Parent[0].RemoveBuff(this);
            while(_Child.Count!=0)
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

            public Unit( Operator op, int priority, BuffData buffData)
            {
                Op = op;
                Priority = priority;
                BuffData = buffData;
            }
        }
    }
}
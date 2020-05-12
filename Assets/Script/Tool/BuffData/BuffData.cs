using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tool.BuffData
{
    public enum Operator
    {
        plus,
        minus,
        multipy,
        divide
    }

    public class BuffData
    {
        private class Buff
        {
            public string name;
            public Operator op;
            public BuffData data;
            public float priority;

            public Buff(string name, Operator op, BuffData data, float priority)
            {
                this.name = name;
                this.op = op;
                this.data = data;
                this.priority = priority;
            }
        }

        public delegate void dataChangedel();
        public event dataChangedel dataChanged;

        private float baseData;

        private float curData;

        private Dictionary<string, Buff> buffRegistered = new Dictionary<string, Buff>();

        private SortedList<float, Buff> buffs = new SortedList<float, Buff>();

        public BuffData(float baseData)
        {
            this.baseData = baseData;
            curData = baseData;
            RefreshData();
        }

        public bool AddBuff(int op, BuffData data, float priority, string name)
        {
            if (buffRegistered.TryGetValue(name, out Buff buff))
                return false;

            Buff newBuff = new Buff(name, (Operator)op, data, priority);
            buffRegistered.Add(name, newBuff);
            buffs.Add(newBuff.priority, newBuff);
            data.dataChanged += RefreshData;

            RefreshData();

            return true;
        }

        public bool RemoveBuff(string name)
        {
            if (!buffRegistered.TryGetValue(name, out Buff buff))
                return false;

            buff.data.dataChanged -= RefreshData;
            buffRegistered.Remove(name);
            buffs.Remove(buff.priority);

            RefreshData();

            return true;
        }

        public void RefreshData()
        {
            curData = baseData;
            foreach (var item in buffs.Values)
            {
                switch (item.op)
                {
                    case Operator.plus:
                        curData += item.data;
                        break;
                    case Operator.minus:
                        curData -= item.data;
                        break;
                    case Operator.divide:
                        curData /= item.data;
                        break;
                    case Operator.multipy:
                        curData *= item.data;
                        break;
                    default:
                        break;
                }
            }
            dataChanged?.Invoke();
        }

        public string[] GetRegistedBuff()
        {
            return buffRegistered.Keys.ToArray();
        }

        public void SetBaseData(float baseData)
        {
            this.baseData = baseData;
            RefreshData();
        }

        public static implicit operator float(BuffData buff)
        {
            return buff.curData;
        }

        public static implicit operator BuffData(float value)
        {
            return new BuffData(value);
        }

        public static float operator +(BuffData v1, BuffData v2)
        {
            return v1.curData + v2.curData;
        }

        public static float operator +(float v1, BuffData v2)
        {
            return v1 + v2.curData;
        }

        public static float operator +(BuffData v1, float v2)
        {
            return v1.curData + v2;
        }

        public static float operator -(BuffData v1, BuffData v2)
        {
            return v1.curData + v2.curData;
        }

        public static float operator -(float v1, BuffData v2)
        {
            return v1 - v2.curData;
        }

        public static float operator -(BuffData v1, float v2)
        {
            return v1.curData - v2;
        }

        public static float operator *(BuffData v1, BuffData v2)
        {
            return v1.curData * v2.curData;
        }

        public static float operator *(float v1, BuffData v2)
        {
            return v1 * v2.curData;
        }

        public static float operator *(BuffData v1, float v2)
        {
            return v1.curData * v2;
        }

        public static float operator /(BuffData v1, BuffData v2)
        {
            return v1.curData / v2.curData;
        }

        public static float operator /(float v1, BuffData v2)
        {
            return v1 / v2.curData;
        }

        public static float operator /(BuffData v1, float v2)
        {
            return v1.curData / v2;
        }

        public static Vector3 operator *(Vector3 v1, BuffData v2)
        {
            return v1 * v2.curData;
        }

        public static Vector3 operator *(BuffData v1, Vector3 v2)
        {
            return v1.curData * v2;
        }

        public override string ToString()
        {
            return curData.ToString();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Thunder.Utility
{
    /// <summary>
    /// 粘滞输入，通过设置按键的生存时间来延长输入
    /// </summary>
    public class StickyInputDic
    {
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
            InputUnit unit = _Inputs[key];
            if (force || value != unit.FloatDef)
            {
                unit.Float = value;
                unit.LifeTimeCount = Time.time;
            }
            _Inputs[key] = unit;
        }

        public void SetInt(string key, int value, bool force = false)
        {
            InputUnit unit = _Inputs[key];
            if (force || value != unit.IntegerDef)
            {
                unit.Integer = value;
                unit.LifeTimeCount = Time.time;
            }

            _Inputs[key] = unit;
        }

        public void SetBool(string key, bool value, bool force = false)
        {
            InputUnit unit = _Inputs[key];
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
                InputUnit unit = _Inputs[inputsKey];
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
    }
}

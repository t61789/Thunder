using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thunder.Tool
{
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
}
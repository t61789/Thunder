using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thunder.Tool
{
    public class DisposableDictionary
    {
        private Dictionary<string, int> intDic = new Dictionary<string, int>();
        private Dictionary<string, float> floatDic = new Dictionary<string, float>();
        private Dictionary<string, Vector3> vectorDic = new Dictionary<string, Vector3>();
        private Dictionary<string, bool> boolDic = new Dictionary<string, bool>();

        public int GetInt(string key, int defaultValue = 0)
        {
            try
            {
                int result = intDic[key];
                intDic[key] = defaultValue;
                return result;
            }
            catch (Exception)
            {
                intDic.Add(key, defaultValue);
                return defaultValue;
            }
        }

        public void SetInt(string key, int value)
        {
            try
            {
                intDic[key] = value;
            }
            catch (Exception)
            {
                intDic.Add(key, value);
            }
        }

        public float GetFloat(string key, float defaultValue = 0)
        {
            try
            {
                float result = floatDic[key];
                floatDic[key] = defaultValue;
                return result;
            }
            catch (Exception)
            {
                floatDic.Add(key, defaultValue);
                return defaultValue;
            }
        }

        public void SetFloat(string key, float value)
        {
            try
            {
                floatDic[key] = value;
            }
            catch (Exception)
            {
                floatDic.Add(key, value);
            }
        }

        public Vector3 GetVector(string key, Vector3 defaultValue = default)
        {
            try
            {
                Vector3 result = vectorDic[key];
                vectorDic[key] = defaultValue;
                return result;
            }
            catch (Exception)
            {
                vectorDic.Add(key, defaultValue);
                return defaultValue;
            }
        }

        public void SetVector(string key, Vector3 value)
        {
            try
            {
                vectorDic[key] = value;
            }
            catch (Exception)
            {
                vectorDic.Add(key, value);
            }
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            try
            {
                bool result = boolDic[key];
                boolDic[key] = defaultValue;
                return result;
            }
            catch (Exception)
            {
                boolDic.Add(key, defaultValue);
                return defaultValue;
            }
        }

        public void SetBool(string key, bool value)
        {
            try
            {
                boolDic[key] = value;
            }
            catch (Exception)
            {
                boolDic.Add(key, value);
            }
        }
    }
}

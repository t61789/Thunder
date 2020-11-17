using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Thunder.Utility
{
    /// <summary>
    /// 定义的函数必须为public，参数为object
    /// </summary>
    public class UiMethods : MonoBehaviour
    {
        private Dictionary<string, MethodInfo> _MethodsBuffer;

        protected virtual void Awake()
        {
            _MethodsBuffer = typeof(UiMethods).GetMethods(
                BindingFlags.CreateInstance |
                BindingFlags.Public).ToDictionary(x => x.Name, x => x);
        }

        public void Invoke(string methodName)
        {
            if (!_MethodsBuffer.TryGetValue(methodName, out var method))
                throw new MethodAccessException($"目标方法 {methodName} 不存在");
            method.Invoke(this, null);
        }
    }
}

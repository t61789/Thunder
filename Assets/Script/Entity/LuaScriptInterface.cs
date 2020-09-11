using LuaInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Thunder.Sys;
using Thunder.Tool;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

#pragma warning disable 649

namespace Thunder.Entity
{
    [Serializable]
    public class LuaScriptInterface : MonoBehaviour
    {
        public string LuaScriptScopePath;

        public string DebugScopePath;

        public bool UseDebugScopePath;

        public bool InitAtStart;

        public object InjectSource;

        protected LuaTable LuaScope;

        public LuaTable Data { private set; get; }

        private static readonly Dictionary<Type, (PropertyInfo[], FieldInfo[])> InjectInfoBuffer =
            new Dictionary<Type, (PropertyInfo[], FieldInfo[])>();

        private static readonly Dictionary<string, LuaTable> LuaScopeBuffer =
            new Dictionary<string, LuaTable>();

        private static readonly Dictionary<(LuaTable, string), LuaFunction> LuaFuncBuffer =
            new Dictionary<(LuaTable, string), LuaFunction>();

        private static bool _ClearBufferRegistered;

        protected virtual void Awake()
        {
            if (InitAtStart)
                InitLuaData(LuaScriptScopePath, DebugScopePath);
        }

        public void InitLuaData(string scriptScope, string debugScriptScope)
        {
            if (string.IsNullOrEmpty(UseDebugScopePath ? debugScriptScope : scriptScope))
            {
                Debug.LogWarning($"{name} 的Lua接口未指定脚本");
                return;
            }

            string[] command;
            if (!UseDebugScopePath)
            {
                command = scriptScope.Split(':');
                Assert.IsTrue(command.Length == 2 &&
                              !string.IsNullOrEmpty(command[0]) &&
                              !string.IsNullOrEmpty(command[1]), $"{name} 的Lua脚本格式有误：{scriptScope}");

                Stable.Lua.ExecuteFile(command[0]);
            }
            else
            {
                using (FileStream fs = File.OpenRead(debugScriptScope))
                {
                    Stable.Lua.ExecuteCommand(new StreamReader(fs).ReadToEnd());
                }
                command = new string[2];
                command[1] = Path.GetFileNameWithoutExtension(debugScriptScope);
            }

            if (!LuaScopeBuffer.TryGetValue(command[1], out LuaScope))
            {
                LuaScope = Stable.Lua.LuaState[command[1]] as LuaTable;
                LuaScopeBuffer.Add(command[1], LuaScope);
            }

            Data = Stable.Lua.GetEmptyTable();
            Inject(Data);
            GetLuaFunc("Init", true)?.Call(Data, this);

            if (_ClearBufferRegistered) return;
            Stable.Lua.StateDisposedEvent.AddListener(ClearBuffer);
            _ClearBufferRegistered = true;
        }

        private void Inject(LuaTable data)
        {
            InjectSource = InjectSource ?? GetComponent<BaseEntity>();
            Assert.IsNotNull(InjectSource, $"对象 {name} 的Lua接口未找到注入源");
            Type curType = InjectSource.GetType();

            if (!InjectInfoBuffer.TryGetValue(curType, out var injectInfo))
            {
                Type monoType = typeof(MonoBehaviour);
                Type objType = typeof(object);
                var proplist = new List<PropertyInfo>();
                var fieldlist = new List<FieldInfo>();

                const BindingFlags search = BindingFlags.Public |
                                            BindingFlags.NonPublic |
                                            BindingFlags.Instance |
                                            BindingFlags.DeclaredOnly |
                                            BindingFlags.Static;

                while (curType != null && curType != objType && curType != monoType)
                {
                    fieldlist.AddRange(curType.GetFields(search)
                        .Where(x => !x.HaveAttribute<DontInjectAttribute>()));
                    proplist.AddRange(curType.GetProperties(search)
                        .Where(x => !x.HaveAttribute<DontInjectAttribute>()));

                    curType = curType.BaseType;
                }

                injectInfo.Item1 = proplist.ToArray();
                injectInfo.Item2 = fieldlist.ToArray();

                InjectInfoBuffer.Add(GetType(), injectInfo);
            }

            foreach (var propertyInfo in injectInfo.Item1)
                data[propertyInfo.Name] = propertyInfo.GetValue(this);
            foreach (var fieldInfo in injectInfo.Item2)
                data[fieldInfo.Name] = fieldInfo.GetValue(this);
        }

        protected virtual void Update()
        {
            if (Data != null) GetLuaFunc("Update")?.Call(Data);
        }

        protected virtual void FixedUpdate()
        {
            if (Data != null) GetLuaFunc("FixedUpdate")?.Call(Data);
        }

        protected virtual void LateUpdate()
        {
            if (Data != null) GetLuaFunc("LateUpdate")?.Call(Data);
        }

        public static void ClearBuffer()
        {
            LuaScopeBuffer.Clear();
            LuaFuncBuffer.Clear();
            InjectInfoBuffer.Clear();
            _ClearBufferRegistered = false;
            Stable.Lua.StateDisposedEvent.RemoveListener(ClearBuffer);
        }

        private void CheckLuaState()
        {
            Assert.IsNotNull(LuaScope, $"{name} 未加载Lua脚本");
        }

        public LuaFunction GetLuaFunc(string func, bool require = false)
        {
            CheckLuaState();

            var key = (LuaScope, func);
            if (LuaFuncBuffer.TryGetValue(key, out var result))
                return result;
            result = LuaScope.GetLuaFunction(func);


            Assert.IsFalse(require && result == null, $"没有找到名为 {func} 的函数");
            LuaFuncBuffer.Add(key, result);
            return result;
        }

        public void CallLuaMethod(string methodName)
        {
            GetLuaFunc(methodName, true)?.Call(Data);
        }
    }
}

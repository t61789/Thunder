using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using LuaInterface;
using Tool;

using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

#pragma warning disable 649

namespace Thunder.Entity
{
    /// <summary>
    ///     Lua脚本路径格式 LuaScriptPath : LuaTable<br />
    /// </summary>
    [Serializable]
    public class LuaInterface : BaseEntity
    {
        private const string UPDATE = "Update";
        private const string FIXED_UPDATE = "FixedUpdate";
        private const string LATE_UPDATE = "LateUpdate";

        private static readonly Dictionary<Type, (PropertyInfo[], FieldInfo[])> _InjectInfoBuffer =
            new Dictionary<Type, (PropertyInfo[], FieldInfo[])>();

        private static readonly Dictionary<string, LuaTable> _LuaScopeBuffer =
            new Dictionary<string, LuaTable>();

        private static readonly Dictionary<(LuaTable, string), LuaFunction> _LuaFuncBuffer =
            new Dictionary<(LuaTable, string), LuaFunction>();

        private static bool _ClearBufferRegistered;

        public string DebugScopePath;

        public bool InitAtStart;

        public object InjectSource;

        protected LuaTable LuaScope;
        public string LuaScriptScopePath;

        public bool UseDebugScopePath;

        public LuaTable Data { private set; get; }

        protected override void Awake()
        {
            base.Awake();
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

                LuaSys.Ins.ExecuteFile(command[0]);
            }
            else
            {
                using (var fs = File.OpenRead(debugScriptScope))
                {
                    LuaSys.Ins.ExecuteCommand(new StreamReader(fs).ReadToEnd());
                }

                command = new string[2];
                command[1] = Path.GetFileNameWithoutExtension(debugScriptScope);
            }

            if (!_LuaScopeBuffer.TryGetValue(command[1], out LuaScope))
            {
                LuaScope = LuaSys.Ins.LuaState[command[1]] as LuaTable;
                _LuaScopeBuffer.Add(command[1], LuaScope);
            }

            Data = LuaSys.Ins.GetEmptyTable();
            Inject(Data);
            GetLuaFunc("Init", true)?.Call(Data, this);

            if (_ClearBufferRegistered) return;
            LuaSys.Ins.StateDisposedEvent.AddListener(ClearBuffer);
            _ClearBufferRegistered = true;
        }

        private void Inject(LuaTable data)
        {
            InjectSource = InjectSource ?? GetComponent<BaseEntity>();
            Assert.IsNotNull(InjectSource, $"对象 {name} 的Lua接口未找到注入源");
            var curType = InjectSource.GetType();

            if (!_InjectInfoBuffer.TryGetValue(curType, out var injectInfo))
            {
                var monoType = typeof(MonoBehaviour);
                var objType = typeof(object);
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

                _InjectInfoBuffer.Add(GetType(), injectInfo);
            }

            foreach (var propertyInfo in injectInfo.Item1)
                data[propertyInfo.Name] = propertyInfo.GetValue(this);
            foreach (var fieldInfo in injectInfo.Item2)
                data[fieldInfo.Name] = fieldInfo.GetValue(this);
        }

        protected virtual void Update()
        {
            if (Data != null) GetLuaFunc(UPDATE)?.Call(Data);
        }

        protected virtual void FixedUpdate()
        {
            if (Data != null) GetLuaFunc(FIXED_UPDATE)?.Call(Data);
        }

        protected virtual void LateUpdate()
        {
            if (Data != null) GetLuaFunc(LATE_UPDATE)?.Call(Data);
        }

        public static void ClearBuffer()
        {
            _LuaScopeBuffer.Clear();
            _LuaFuncBuffer.Clear();
            _InjectInfoBuffer.Clear();
            _ClearBufferRegistered = false;
            LuaSys.Ins.StateDisposedEvent.RemoveListener(ClearBuffer);
        }

        private void CheckLuaState()
        {
            Assert.IsNotNull(LuaScope, $"{name} 未加载Lua脚本");
        }

        public LuaFunction GetLuaFunc(string func, bool require = false)
        {
            CheckLuaState();

            var key = (LuaScope, func);
            if (_LuaFuncBuffer.TryGetValue(key, out var result))
                return result;
            result = LuaScope.GetLuaFunction(func);

            Assert.IsFalse(require && result == null, $"没有找到名为 {func} 的函数");
            _LuaFuncBuffer.Add(key, result);
            return result;
        }

        public void CallLuaFunc(string methodName)
        {
            GetLuaFunc(methodName, true)?.Call(Data);
        }
    }
}
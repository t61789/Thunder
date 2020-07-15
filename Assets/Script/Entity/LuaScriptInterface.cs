using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LuaInterface;
using Thunder.Sys;
using Thunder.Tool;
using UnityEngine;
using UnityEngine.Assertions;

#pragma warning disable 649

namespace Thunder.Entity
{
    public class LuaScriptInterface:MonoBehaviour
    {
        [DontInject]
        [SerializeField]
        private string _LuaScriptScopePath;

        [DontInject]
        protected LuaTable LuaScope;

        [DontInject]
        public LuaTable Data;

        [DontInject]
        private static readonly Dictionary<Type, (PropertyInfo[], FieldInfo[])> InjectInfoBuffer =
            new Dictionary<Type, (PropertyInfo[], FieldInfo[])>();

        [DontInject]
        private static readonly Dictionary<string, LuaTable> LuaScopeBuffer =
            new Dictionary<string, LuaTable>();

        [DontInject]
        private static readonly Dictionary<(LuaTable, string), LuaFunction> LuaFuncBuffer =
            new Dictionary<(LuaTable, string), LuaFunction>();

        [DontInject]
        private static bool _ClearBufferRegistered;

        protected virtual void Awake()
        {
            InitData(_LuaScriptScopePath);
        }

        private void InitData(string scriptScope)
        {
            if (string.IsNullOrEmpty(scriptScope))
            {
                Debug.LogWarning($"{name} 的Lua接口未指定脚本");
                return;
            }

            var command = scriptScope.Split(':');
            Assert.IsTrue(command.Length == 2 &&
                          !string.IsNullOrEmpty(command[0]) &&
                          !string.IsNullOrEmpty(command[1]), $"{name} 的Lua脚本格式有误：{scriptScope}");

            Stable.Lua.ExecuteFile(command[0]);
            if (!LuaScopeBuffer.TryGetValue(command[1], out LuaScope))
            {
                LuaScope = Stable.Lua.LuaState[command[1]] as LuaTable;
                LuaScopeBuffer.Add(command[1], LuaScope);
            }

            Data = Stable.Lua.GetEmptyTable();
            Data["CSharp"] = this;
            Inject(Data);
            GetLuaFunc("Init")?.Call(Data, this);

            if (_ClearBufferRegistered) return;
            Stable.Lua.StateDisposedEvent.AddListener(ClearBuffer);
            _ClearBufferRegistered = true;
        }

        private void Inject(LuaTable data)
        {
            Type curType = GetType();

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

        protected class DontInjectAttribute : Attribute
        {

        }
    }
}

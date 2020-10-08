using LuaInterface;
using System.Collections.Generic;
using System.Linq;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Thunder.Sys
{
    public class LuaSys:IBaseSys
    {
        public static LuaSys Ins { get; private set; }

        private static readonly string DefaultBundle = Paths.LuaBundleD + Paths.Normal;

        private const string UtilityFunc =
@"Utility={}
Utility.GetEmptyTable = function()
    return {}
end";

        private static LuaTable _UtilityScope;

        public LuaState LuaState
        {
            get
            {
                Assert.IsTrue(Started, "LuaState未启动，不可调用");
                return _LuaState;
            }
        }

        private LuaState _LuaState;

        public static bool Started { get; private set; }

        private readonly HashSet<AssetId> _Executed = new HashSet<AssetId>();
        private readonly Dictionary<AssetId, string> _LoadedFile = new Dictionary<AssetId, string>();

        public UnityEvent StateDisposedEvent = new UnityEvent();

        public LuaSys()
        {
            Ins = this;
            StartLua();
            Application.quitting += Dispose;
        }

        public void StartLua()
        {
            Assert.IsFalse(Started, "不可重复启动LuaState，请先Dispose");

            _LuaState = new LuaState();
            _LuaState.Start();

            LuaBinder.Bind(_LuaState);
            DelegateFactory.Init();

            Started = true;

            ExecuteCommand(UtilityFunc);
            _UtilityScope = LuaState["Utility"] as LuaTable;
        }

        public void Dispose()
        {
            Assert.IsTrue(Started, "不可重复Dispose，请先启动LuaState");
            _UtilityScope = null;
            _LuaState.Dispose();
            StateDisposedEvent?.Invoke();
            Application.quitting -= Dispose;
            _Executed.Clear();
            _LoadedFile.Clear();
            Started = false;
        }

        public void ExecuteBundle(string bundleGroup, string bundle)
        {
            Assert.IsTrue(Started, "LuaState未启动");

            for (int i = 0; i < 2; i++)
            {
                AssetId id = new AssetId(bundleGroup, bundle, null, DefaultBundle);
                bool have = false;
                foreach (var keyValuePair in _LoadedFile.Where(x => AssetId.BundleEquals(x.Key, id)))
                {
                    have = true;
                    _LuaState.DoString(keyValuePair.Value);
                }

                if (have) return;

                Assert.IsTrue(LoadFromBundle(id), $"{id.BundleGroup}!{id.Bundle} 中没有文件");
            }

            _LuaState.CheckTop();
        }
        public void ExecuteFile(string path, bool require = true)
        {
            ExecuteFile(AssetId.Create(path, DefaultBundle), require);
        }

        public void ExecuteFile(string bundleGroup, string bundle, string name, bool require = true)
        {
            ExecuteFile(new AssetId(bundleGroup, bundle, name, DefaultBundle), require);
        }

        public void ExecuteFile(AssetId id, bool require = true)
        {
            Assert.IsTrue(Started, "LuaState未启动");

            for (int i = 0; i < 2; i++)
            {
                if (_LoadedFile.TryGetValue(id, out string value))
                {
                    if (require && _Executed.Contains(id)) return;
                    _LuaState.DoString(value);
                    _Executed.Add(id);
                    return;
                }

                LoadFromBundle(id);

                Assert.IsTrue(_LoadedFile.ContainsKey(id), $"{id.BundleGroup}!{id.Bundle} 中不存在名为 {id.Name} 的lua文件");
            }

            _LuaState.CheckTop();
        }

        public void ExecuteCommand(string cmd)
        {
            Assert.IsTrue(Started, "LuaState未启动");

            _LuaState.DoString(cmd);

            _LuaState.CheckTop();
        }

        private bool LoadFromBundle(AssetId id)
        {
            bool have = false;
            foreach (var textAsset in BundleSys.Ins.GetAllAsset<TextAsset>(id.BundleGroup, id.Bundle))
            {
                id.Name = textAsset.name;
                if (_LoadedFile.ContainsKey(id)) continue;
                _LoadedFile.Add(id, textAsset.text);
                have = true;
            }
            BundleSys.Ins.ReleaseBundle(id.BundleGroup, id.Bundle);
            return have;
        }

        public LuaTable GetEmptyTable()
        {
            Assert.IsTrue(Started, "LuaState未启动");
            return _UtilityScope.Invoke<LuaTable>("GetEmptyTable");
        }

        public void OnSceneEnter(string preScene, string curScene)
        {
            
        }

        public void OnSceneExit(string curScene)
        {
        }

        public void OnApplicationExit()
        {
        }
    }
}
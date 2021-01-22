using System.Collections.Generic;
using System.Linq;
using Framework;
using LuaInterface;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Tool
{
    public class LuaSys : IBaseSys
    {
        private const string UTILITY_FUNC =
            @"Utility={}
Utility.GetEmptyTable = function()
    return {}
end";

        private static LuaTable _UtilityScope;

        private readonly HashSet<AssetId> _Executed = new HashSet<AssetId>();
        private readonly Dictionary<AssetId, string> _LoadedFile = new Dictionary<AssetId, string>();

        private LuaState _LuaState;

        public UnityEvent StateDisposedEvent = new UnityEvent();

        public LuaSys()
        {
            Ins = this;
            StartLua();
            Application.quitting += Dispose;
        }

        public static LuaSys Ins { get; private set; }

        public LuaState LuaState
        {
            get
            {
                Assert.IsTrue(Started, "LuaState未启动，不可调用");
                return _LuaState;
            }
        }

        public static bool Started { get; private set; }

        public void OnSceneEnter(string preScene, string curScene)
        {
        }

        public void OnSceneExit(string curScene)
        {
        }

        public void OnApplicationExit()
        {
        }

        public void StartLua()
        {
            Assert.IsFalse(Started, "不可重复启动LuaState，请先Dispose");

            _LuaState = new LuaState();
            _LuaState.Start();

            LuaBinder.Bind(_LuaState);
            DelegateFactory.Init();

            Started = true;

            ExecuteCommand(UTILITY_FUNC);
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

            for (var i = 0; i < 2; i++)
            {
                var id = new AssetId(bundle, null);
                var have = false;
                foreach (var keyValuePair in _LoadedFile.Where(x => x.Key.Bundle== id.Bundle))
                {
                    have = true;
                    _LuaState.DoString(keyValuePair.Value);
                }

                if (have) return;

                Assert.IsTrue(LoadFromBundle(id), $"{id.Bundle} 中没有文件");
            }

            _LuaState.CheckTop();
        }

        public void ExecuteFile(string path, bool require = true)
        {
            ExecuteFile(AssetId.Parse(path), require);
        }

        public void ExecuteFile(AssetId id, bool require = true)
        {
            Assert.IsTrue(Started, "LuaState未启动");

            for (var i = 0; i < 2; i++)
            {
                if (_LoadedFile.TryGetValue(id, out var value))
                {
                    if (require && _Executed.Contains(id)) return;
                    _LuaState.DoString(value);
                    _Executed.Add(id);
                    return;
                }

                LoadFromBundle(id);

                Assert.IsTrue(_LoadedFile.ContainsKey(id), $"{id.Bundle} 中不存在名为 {id.Name} 的lua文件");
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
            var have = false;
            foreach (var textAsset in BundleSys.GetAllAsset<TextAsset>(id.Bundle))
            {
                id.Name = textAsset.name;
                if (_LoadedFile.ContainsKey(id)) continue;
                _LoadedFile.Add(id, textAsset.text);
                have = true;
            }

            BundleSys.ReleaseBundle( id.Bundle);
            return have;
        }

        public LuaTable GetEmptyTable()
        {
            Assert.IsTrue(Started, "LuaState未启动");
            return _UtilityScope.Invoke<LuaTable>("GetEmptyTable");
        }
    }
}
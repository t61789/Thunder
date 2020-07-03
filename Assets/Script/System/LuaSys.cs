using System;
using Assets.Script.PublicScript;
using LuaInterface;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Assets.Script.System
{
    public class LuaSys
    {
        private static readonly string DefaultBundle = BundleSys.LuaBundleD + BundleSys.Normal;

        public LuaState LuaState
        {
            get
            {
                Assert.IsTrue(_Started, "LuaState未启动，不可调用");
                return _LuaState;
            }
        }

        private LuaState _LuaState;
        private static bool _Started;

        public LuaSys()
        {
            StartLua();
        }

        public void StartLua()
        {
            Assert.IsFalse(_Started, "不可重复启动LuaState，请先Dispose");

            _LuaState = new LuaState();
            _LuaState.Start();

            LuaBinder.Bind(_LuaState);
            DelegateFactory.Init();

            Application.quitting += Dispose;

            _Started = true;
        }

        public void Dispose()
        {
            Assert.IsTrue(_Started, "不可重复请先Dispose，请先启动LuaState");

            _LuaState.Dispose();

            Application.quitting -= Dispose;
            _Started = false;
        }

        public void LoadFromBundle(string bundleGroup,string bundle)
        {
            AssetId id = new AssetId(bundleGroup, bundle, null, DefaultBundle);
            foreach (var textAsset in System.bundle.GetAllAsset<TextAsset>(id.BundleGroup,id.Bundle))
                _LuaState.DoString(textAsset.text);
            System.bundle.ReleaseBundle(id.BundleGroup,id.Bundle);
        }

        public void LoadFromAsset(string path)
        {
            LoadFromAsset(AssetId.CreateAssetId(path,DefaultBundle));
        }

        public void LoadFromAsset(string bundleGroup, string bundle,string name)
        {
            LoadFromAsset(new AssetId(bundleGroup, bundle, name,DefaultBundle));
        }

        private void LoadFromAsset(AssetId id)
        {
            _LuaState.DoString(System.bundle.GetAsset<TextAsset>(id.BundleGroup, id.Bundle, id.Name).text);
        }
    }
}
﻿using System;
using System.Collections;

using Thunder.Tool;
using Thunder.Tool.ObjectPool;
using Thunder.UI;
using Thunder.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Thunder.Sys
{
    public class Stable : MonoBehaviour
    {
        public bool RedirectLog;

        public static Stable Instance;

        public static GameObject PublicVar;
        public static ObjectPool ObjectPool;
        public static Transform Container;

        public static BundleSys Bundle;
        public static ValueSys Value;
        public static ControlSys Control;
        public static ConsoleWindow ConsoleWindow;
        public static CampSys Camp;
        public static UiSys Ui;
        public static DataBaseSys DataBase;
        public static CameraController MainCamera;
        public static SaveSys Save;
        public static LuaSys Lua;

        private bool _Loading;
        private AsyncOperation _LoadingAo;
        private LogPanel _LoadingLoadPanel;

        public TextMeshProUGUI Log;

        private void Awake()
        {
            Instance = this;

            switch (SceneManager.GetActiveScene().name)
            {
                case "StartScene":
                    PublicVar = gameObject;
                    Bundle?.ReleaseAllBundleGroup();
                    Bundle = new BundleSys();
                    ObjectPool = gameObject.AddComponent<ObjectPool>();
                    Container = GameObject.Find("Container").transform;
                    Lua = new LuaSys();
                    Ui = new UiSys();
                    DataBase = new DataBaseSys();
                    break;

                case "CreateSaveScene":
                    ObjectPool = gameObject.AddComponent<ObjectPool>();
                    Container = GameObject.Find("Container").transform;
                    Ui = GetComponent<UiSys>();
                    break;

                case "LevelScene":
                    PublicVar = gameObject;
                    ObjectPool = gameObject.AddComponent<ObjectPool>();
                    Container = GameObject.Find("Container").transform;
                    Value = new ValueSys();
                    Ui = GetComponent<UiSys>();
                    break;
                case "TestScene":
                    PublicVar = gameObject;
                    Bundle = new BundleSys();
                    Lua = new LuaSys();
                    DataBase = new DataBaseSys();
                    ObjectPool = gameObject.AddComponent<ObjectPool>();
                    Container = GameObject.Find("Container").transform;
                    Ui = new UiSys();
                    Camp = new CampSys();
                    MainCamera = Camera.main.transform.GetComponent<CameraController>();
                    Value = new ValueSys();
                    Control = gameObject.AddComponent<ControlSys>();
                    break;
                case "BattleScene":
                    PublicVar = gameObject;
                    ObjectPool = GetComponent<ObjectPool>();
                    Container = GameObject.Find("Container").transform;
                    MainCamera = GameObject.Find("MainCamera").GetComponent<CameraController>();
                    Value = new ValueSys();
                    Control = gameObject.AddComponent<ControlSys>();
                    Camp = new CampSys();
                    Ui = new UiSys();
                    break;
                case "LuaTestScene":
                    Bundle = new BundleSys();
                    Lua = new LuaSys();
                    Control = gameObject.AddComponent<ControlSys>();
                    break;
            }

            GC.Collect();
        }

        private void Update()
        {
            if (_Loading)
            {
                _LoadingLoadPanel.SetText("loading: " + _LoadingAo.progress);
            }
        }

        public void LoadSceneAsync(string sceneName)
        {
            _Loading = true;
            _LoadingAo = SceneManager.LoadSceneAsync(sceneName);
            _LoadingLoadPanel = Ui.OpenUi("logPanel", UiInitType.CenterParent) as LogPanel;
            StartCoroutine(LoadScene(_LoadingAo));
        }

        private IEnumerator LoadScene(AsyncOperation ao)
        {
            ao.allowSceneActivation = false;
            while (ao.progress < 0.9f)
                yield return null;
            ao.allowSceneActivation = true;
        }

        public static void DepCheck(Type name, object target, bool exists)
        {
            if (exists && target == null)
                throw new Exception("System " + name + " dependence on " + target.ToString() + " but it doesn't exist");
            if (!exists && target != null)
                throw new Exception("System " + name + " needs " + target.ToString() + " to be empty, but it exist");
        }

        public void OnApplicationQuit()
        {
            if(LuaSys.Started)
                Lua.Dispose();
        }

        public void Test(string cmd)
        {
            Stable.Lua.ExecuteFile("BaseUi");
            Stable.Lua.ExecuteCommand(cmd);
        }
    }

    public interface ISys
    {
        void Reset();
    }
}


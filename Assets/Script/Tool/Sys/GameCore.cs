using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Thunder.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tool
{
    /// <summary>
    /// 总管理器，用于给各系统初始化和分发消息
    /// </summary>
    public class GameCore : MonoBehaviour
    {
        // 是否启用异常记录功能
        public static bool SaveLog =
#if UNITY_EDITOR
            false;
#elif UNITY_STANDALONE_WIN
            true;
#else
            false;
#endif

        public static GameCore Ins { get; private set; }
        public static Transform Container { get; private set; }

        private AsyncOperation _LoadingAo;
        private IBaseSys[] _Sys;
        private string _CurScene;

        private void Awake()
        {
            Ins = this;

            if (SaveLog)
            {
                if (SaveLog)
                    Application.logMessageReceived += (condition, stackTrace, logType) =>
                    {
                        File.AppendAllText(Paths.LogPath,
                            $"[condition]\n{condition}\n[stackTrace]\n{stackTrace}\n");
                    };
                SaveLog = false;
            }// 如果开启了异常记录功能，就会将每一个异常记录到指定的日志文件中

            // 初始化各系统并将他们象转换成数组
            var sys = new IBaseSys[]
            {
                new BundleSys(),
                new DataBaseSys(),
                new CampSys(),
                gameObject.AddComponent<ControlSys>(),
                new LuaSys(),
                new UISys(),
                new ValueSys(),
                gameObject.AddComponent<ObjectPool>(),
                new TextSys(),
            };
            var customSys = Config.Init();
            _Sys = new IBaseSys[sys.Length+customSys.Length];
            Array.Copy(sys,0,_Sys,0,sys.Length);
            Array.Copy(customSys, 0, _Sys, sys.Length, customSys.Length);

            // 避免在场景切换时摧毁系统物体
            DontDestroyOnLoad(gameObject);

            // 注册场景加载/卸载事件
            SceneManager.sceneLoaded += OnEnterScene;
            SceneManager.sceneUnloaded += OnExitScene;
        }

        // 异步加载场景
        public void LoadSceneAsync(string sceneName)
        {
            foreach (var syss in _Sys)
                syss.OnSceneExit(_CurScene);

            _LoadingAo = SceneManager.LoadSceneAsync(sceneName);
            StartCoroutine(LoadScene(_LoadingAo, sceneName));
        }

        // 加载后启用场景的协程
        private IEnumerator LoadScene(AsyncOperation ao, string newScene)
        {
            ao.allowSceneActivation = false;
            while (ao.progress < 0.9f)
                yield return null;
            ao.allowSceneActivation = true;
        }

        // 以下为方法为事件分发
        private void OnExitScene(Scene scene)
        {
            foreach (var syss in _Sys)
                syss.OnSceneExit(_CurScene);
        }

        private void OnEnterScene(Scene scene, LoadSceneMode mode)
        {
            Container = new GameObject("Container").transform;
            var targetScene = scene.name;
            foreach (var s in _Sys)
                s.OnSceneEnter(_CurScene, targetScene);
            _CurScene = targetScene;
        }

        private void OnApplicationQuit()
        {
            foreach (var s in _Sys)
                s.OnApplicationExit();
        }
    }
    public interface IBaseSys
    {
        void OnSceneEnter(string preScene, string curScene);
        void OnSceneExit(string curScene);
        void OnApplicationExit();
    }
}
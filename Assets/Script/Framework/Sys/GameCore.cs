using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
    /// <summary>
    /// 总管理器，用于给各系统初始化和分发消息
    /// </summary>
    public class GameCore : MonoBehaviour
    {
        // 是否启用异常记录功能
        public static bool SaveLog =
            !Platform.CurPlatform.HasFlag(Platforms.Editor) &&
            Platform.CurPlatform.HasFlag(Platforms.StandaloneWin);

        public static GameCore Ins { get; private set; }
        public static Transform Container { get; private set; }

        private static AsyncOperation _LoadingAo;
        private static IBaseSys[] _Sys;
        private static string _CurScene;

        private void Awake()
        {
            if(Ins!=null)
                throw new InitDuplicatelyException();
            Ins = this;

            if (SaveLog)
            {
                Application.logMessageReceived += (condition, stackTrace, logType) =>
                {
                    File.AppendAllText(Paths.LogPath,
                        $"[condition]\n{condition}\n[stackTrace]\n{stackTrace}\n");
                };
            }// 如果开启了异常记录功能，就会将每一个异常记录到指定的日志文件中

            // 初始化各系统并将他们转换成数组
            var sys = new IBaseSys[]
            {
                new BundleSys(),
                new DataBaseSys(),
                gameObject.AddComponent<ControlSys>(),
                gameObject.AddComponent<GameObjectPool>(),
                gameObject.AddComponent<InstructionBalancing>(),
                new UiSys(),
                new ValueSys(),
                new TextSys(),
                new ItemSys()
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

        private void OnApplicationQuit()
        {
            foreach (var s in _Sys)
                s.OnApplicationExit();
        }

        // 异步加载场景
        public static void LoadSceneAsync(string sceneName)
        {
            foreach (var syss in _Sys)
                syss.OnSceneExit(_CurScene);

            _LoadingAo = SceneManager.LoadSceneAsync(sceneName);
            Ins.StartCoroutine(LoadScene(_LoadingAo, sceneName));
        }

        // 加载后启用场景的协程
        private static IEnumerator LoadScene(AsyncOperation ao, string newScene)
        {
            ao.allowSceneActivation = false;
            while (ao.progress < 0.9f)
                yield return null;
            ao.allowSceneActivation = true;
        }

        // 以下为方法为事件分发
        private static void OnExitScene(Scene scene)
        {
            foreach (var syss in _Sys)
                syss.OnSceneExit(_CurScene);
        }

        private static void OnEnterScene(Scene scene, LoadSceneMode mode)
        {
            Container = new GameObject("Container").transform;
            var targetScene = scene.name;
            foreach (var s in _Sys)
                s.OnSceneEnter(_CurScene, targetScene);
            _CurScene = targetScene;
        }
    }
    public interface IBaseSys
    {
        void OnSceneEnter(string preScene, string curScene);
        void OnSceneExit(string curScene);
        void OnApplicationExit();
    }
}
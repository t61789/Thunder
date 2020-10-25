using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Thunder.Tool;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Thunder.Sys
{
    /// <summary>
    /// 总管理器，用于给各系统初始化和分发消息
    /// </summary>
    public class Stable : MonoBehaviour
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

        public static Stable Ins { get; private set; }
        public static Transform Container { get; private set; }

        private BundleSys _BundleSys;
        private CampSys _CampSys;
        private ControlSys _ControlSys;
        private DataBaseSys _DataBaseSys;
        private LuaSys _LuaSys;
        //private SaveSys _SaveSys;
        private UISys _UISys;
        private ValueSys _ValueSys;
        private ObjectPool _ObjectPool;
        private TextSys _TextSys;
        private ItemSys _ItemSys;

        private AsyncOperation _LoadingAo;
        private IBaseSys[] _Sys;
        private string _CurScene;

        private void Awake()
        {
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

            // 初始化各系统
            Ins = this;
            _BundleSys = new BundleSys();
            _DataBaseSys = new DataBaseSys();
            _CampSys = new CampSys();
            _ControlSys = gameObject.AddComponent<ControlSys>();
            _LuaSys = new LuaSys();
            _UISys = new UISys();
            _CampSys = new CampSys();
            _ValueSys = new ValueSys();
            _ObjectPool = gameObject.AddComponent<ObjectPool>();
            _TextSys = new TextSys(_DataBaseSys);
            _ItemSys = new ItemSys(
                _DataBaseSys[GlobalSettings.ItemInfoTableName],
                DataBaseSys.AvaliableDataType);

            // 将继承了IBaseSys接口的所有成员对象转换成数组
            _Sys = (from field
                    in GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                where field.FieldType.GetInterface("IBaseSys") != null
                select (IBaseSys) field.GetValue(this)).ToArray();

            // 一些初始化
            RandomRewardGenerator.ConstractDic(_DataBaseSys["reward"]);

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
}
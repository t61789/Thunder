using System;
using System.Collections;
using Assets.Script.PublicScript;
using Assets.Script.Tool;
using Assets.Script.Tool.ObjectPool;
using Assets.Script.UI;
using Assets.Script.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Script.System
{
    public class System : MonoBehaviour
    {
        public bool RedirectLog;

        public static System instance;

        public static GameObject publicVar;
        public static ObjectPool objectPool;
        public static Transform container;

        public static BundleSys bundle;
        public static ValueSys value;
        public static ControlManager control;
        public static SkillManager skill;
        public static ConsoleWindow consoleWindow;
        public static CampManager camp;
        public static UiSys UiSys;
        public static DataBaseSys dataBase;
        public static CameraController mainCamera;
        public static GameModeManager gameMode;
        public static SaveSys saveManager;
        public static PlayerManager player;
        public static LevelManager level;
        public static LuaSys lua;

        private bool loading;
        private AsyncOperation loadingAO;
        private LogPanel loadingLoadPanel;

        public TextMeshProUGUI Log;

        private void Awake()
        {
            instance = this;

            switch (SceneManager.GetActiveScene().name)
            {
                case "StartScene":
                    publicVar = gameObject;
                    bundle?.ReleaseAllBundleGroup();
                    bundle = new BundleSys();
                    objectPool = GetComponent<ObjectPool>();
                    container = GameObject.Find("Container").transform;
                    UiSys = GetComponent<UiSys>();
                    dataBase = new DataBaseSys();
                    lua = new LuaSys();
                    break;

                case "CreateSaveScene":
                    objectPool = GetComponent<ObjectPool>();
                    container = GameObject.Find("Container").transform;
                    UiSys = GetComponent<UiSys>();
                    break;

                case "LevelScene":
                    publicVar = gameObject;
                    objectPool = GetComponent<ObjectPool>();
                    container = GameObject.Find("Container").transform;
                    value = new ValueSys();
                    UiSys = GetComponent<UiSys>();
                    gameMode = new GameModeManager();
                    level = new LevelManager();
                    break;
                case "TestScene":
                    publicVar = gameObject;
                    bundle = new BundleSys();
                    objectPool = GetComponent<ObjectPool>();
                    container = GameObject.Find("Container").transform;
                    UiSys = GetComponent<UiSys>();
                    dataBase = new DataBaseSys();
                    value = new ValueSys();
                    control = new ControlManager();
                    break;
                case "BattleScene":
                    publicVar = gameObject;
                    objectPool = GetComponent<ObjectPool>();
                    container = GameObject.Find("Container").transform;
                    mainCamera = GameObject.Find("MainCamera").GetComponent<CameraController>();
                    value = new ValueSys();
                    player = new PlayerManager();
                    skill = new SkillManager();
                    control = new ControlManager();
                    camp = new CampManager();
                    UiSys = GetComponent<UiSys>();
                    gameMode = new GameModeManager();
                    level = new LevelManager();
                    break;
                default:
                    break;
            }

            GC.Collect();
        }

        private void Update()
        {
            if (loading)
            {
                loadingLoadPanel.SetText("loading: " + loadingAO.progress);
            }
        }

        public void LoadSceneAsync(string sceneName)
        {
            loading = true;
            loadingAO = SceneManager.LoadSceneAsync(sceneName);
            loadingLoadPanel = UiSys.OpenUi<LogPanel>("logPanel", UiInitAction.CenterParent);
            StartCoroutine(LoadScene(loadingAO));
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
            else if (!exists && target != null)
                throw new Exception("System " + name + " needs " + target.ToString() + " to be empty, but it exist");
        }
    }
}


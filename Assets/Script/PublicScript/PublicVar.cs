using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using Tool;
using Tool.ObjectPool;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PublicVar : MonoBehaviour
{
    public bool RedirectLog;

    public static PublicVar instance;

    public static GameObject publicVar;
    public static ObjectPool objectPool;
    public static Transform container;

    public static BundleManager bundle;
    public static ValueManager value;
    public static ControlManager control;
    public static SkillManager skill;
    public static ConsoleWindow consoleWindow;
    public static CampManager camp;
    public static UIManager uiManager;
    public static DataBaseManager dataBase;
    public static CameraController mainCamera;
    public static GameModeManager gameMode;
    public static SaveManager saveManager;
    public static PlayerManager player;

    public static string gameDocument;

    private bool loading;
    private AsyncOperation loadingAO;
    private LogPanel loadingLoadPanel;

    static PublicVar()
    {
        gameDocument = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
            Path.DirectorySeparatorChar +
            "MyGames" +
            Path.DirectorySeparatorChar +
            "Thunder";
    }

    private void Awake()
    {
        instance = this;

        switch (SceneManager.GetActiveScene().name)
        {
            case "StartScene":
                publicVar = gameObject;
                if (bundle != null)
                    bundle.ReleaseAllBundle(true);
                bundle = new BundleManager();
                objectPool = GetComponent<ObjectPool>();
                container = GameObject.Find("Container").transform;
                uiManager = GetComponent<UIManager>();
                dataBase = new DataBaseManager();
                break;

            case "CreateSaveScene":
                objectPool = GetComponent<ObjectPool>();
                container = GameObject.Find("Container").transform;
                uiManager = GetComponent<UIManager>();
                break;

            case "MainScene":
                publicVar = gameObject;
                objectPool = GetComponent<ObjectPool>();
                container = GameObject.Find("Container").transform;
                mainCamera = GameObject.Find("MainCamera").GetComponent<CameraController>();
                value = new ValueManager();
                player = new PlayerManager();
                skill = new SkillManager();
                control = new ControlManager();
                camp = new CampManager();
                uiManager = GetComponent<UIManager>();
                gameMode = new GameModeManager();
                break;

            default:
                break;
        }

        GC.Collect();


#if UNITY_EDITOR_WIN
#else
		consoleWindow = new ConsoleWindow();
#endif
    }

    private void Update()
    {
        if (loading)
        {
            loadingLoadPanel.SetText("loading: "+loadingAO.progress);
        }
    }

    public void LoadSceneAsync(string sceneName)
    {
        loading = true;
        loadingAO = SceneManager.LoadSceneAsync(sceneName);
        loadingLoadPanel = uiManager.OpenUI<LogPanel>("logPanel", UIInitAction.CenterParent);
        StartCoroutine(LoadScene(loadingAO));
    }

    private IEnumerator LoadScene(AsyncOperation ao)
    {
        ao.allowSceneActivation = false;
        while (ao.progress<0.9f)
            yield return null;
        ao.allowSceneActivation = true;
    }
}


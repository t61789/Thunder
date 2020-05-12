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

    public static GameObject publicVar;
    public static GameObject curPlayer;
    public static ObjectPool objectPool;
    public static Transform container;
    public static GameObject player;
    public static BundleManager bundleManager;
    public static ValueManager valueManager;
    public static ControlManager controlManager;
    public static SkillManager skillManager;
    public static ConsoleWindow consoleWindow;
    public static CampManager campManager;
    public static UIManager uiManager;
    public static DataBaseManager dataBaseManager;
    public static CameraController mainCamera;
    public static GameModeManager gameModeManager;

    public static string gameDocument;
    public static Hashtable dataDeliver;

    static PublicVar()
    {
        dataDeliver = new Hashtable();
        gameDocument = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
            Path.DirectorySeparatorChar +
            "MyGames" +
            Path.DirectorySeparatorChar +
            "Thunder";
    }

    private void Awake()
    {
        //publicVar = gameObject;

        // bundleManager = new BundleManager();
        // dataBaseManager = new DataBaseManager();
        // objectPool = GetComponent<ObjectPool>();
        // container = GameObject.Find("Container").transform;
        // mainCamera = GameObject.Find("MainCamera").GetComponent<CameraController>();
        //player = GameObject.Find("Player");
        // valueManager = new ValueManager();
        // skillManager = new SkillManager();
        // controlManager = new ControlManager();
        // campManager = new CampManager();
        
        //{
        //    uiManager = GetComponent<UIManager>();
        //    uiManager.Init();
        //}
        // gameModeManager = new GameModeManager();

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0:
                {
                    publicVar = gameObject;

                    bundleManager = new BundleManager();

                    objectPool = GetComponent<ObjectPool>();
                    container = GameObject.Find("Container").transform;

                    uiManager = GetComponent<UIManager>();
                    uiManager.Init();

                    dataBaseManager = new DataBaseManager();

                    SaveManager.CreateSaveDir("Shit");
                }
                break;
            case 1:
                {
                    publicVar = gameObject;
                    dataBaseManager = new DataBaseManager();
                    objectPool = GetComponent<ObjectPool>();
                    container = GameObject.Find("Container").transform;
                    mainCamera = GameObject.Find("MainCamera").GetComponent<CameraController>();
                    player = GameObject.Find("Player");
                    valueManager = new ValueManager();
                    skillManager = new SkillManager();
                    controlManager = new ControlManager();
                    campManager = new CampManager();
                    uiManager = GetComponent<UIManager>();
                    uiManager.Init();
                    gameModeManager = new GameModeManager();
                }
                break;

            default:
                break;
        }


#if UNITY_EDITOR_WIN
#else
		consoleWindow = new ConsoleWindow();
#endif
    }
}


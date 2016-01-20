using UnityEngine;
using System.Collections;
using LuaInterface;

public class GameManager : MonoBehaviour
{
    LuaScriptMgr luaMgr;
    BundleManager bundleMgr;
    public static GameManager instance = null;

    public static GameManager Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject obj = new GameObject("Game Manager");
                instance = obj.AddComponent<GameManager>();
                instance.Init();
				DontDestroyOnLoad(obj);
            }

            return instance;
        }
    }

    public BundleManager BundleMgr
    {
        get { return bundleMgr; }
    }

    public LuaScriptMgr LuaMgr
    {
        get { return luaMgr; }
    }

    void Init()
    {
        bundleMgr = gameObject.AddComponent<BundleManager>();
        luaMgr = new LuaScriptMgr();
        LuaStatic.Load = Utils.LuaLoader;

        bundleMgr.Init();
        Utils.DoFile("Init");
    }

    void Start()
    {
        luaMgr.Start();

        Object guiPrefab = BundleMgr.LoadAsset("Prefabs/GUI.prefab");
        GameObject gui = GameObject.Instantiate(guiPrefab) as GameObject;
        gui.name = "GUI";

        Object guanyuPrefab = BundleMgr.LoadAsset("Prefabs/guanyu.prefab");
        GameObject guanyu = GameObject.Instantiate(guanyuPrefab) as GameObject;
        guanyu.name = "guanyu";
        //guanyu.transform.SetParent(gui.transform);
        //guanyu.transform.localScale = new Vector3(1, 1, 1);

        Object mainCityPanel = BundleMgr.LoadAsset("Prefabs/MainCity.prefab");
        GameObject mainCity = GameObject.Instantiate(mainCityPanel) as GameObject;
        mainCity.name = "MainCity";
        mainCity.transform.SetParent(gui.transform);
        mainCity.transform.localScale = new Vector3(1, 1, 1);

        //BundleMgr.LoadAsyncAsset("Prefabs/MainCity.prefab", callback);
    }
    
    void callback(Object obj)
    {
        GameObject mainCity = GameObject.Instantiate(obj) as GameObject;
        mainCity.name = "MainCity";
        GameObject uiroot = GameObject.Find("UI Root");
        mainCity.transform.SetParent(uiroot.transform);
        mainCity.transform.localScale = new Vector3(1, 1, 1);
    }
}

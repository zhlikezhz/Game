using UnityEngine;
using System.Collections;
using LuaInterface;

public class GameEngine : MonoBehaviour
{
    LuaScriptMgr luaMgr;
    AssetBundleMgr bundleMgr;
    public static GameEngine instance = null;

    public static GameEngine Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject obj = new GameObject("Game Engine");
                instance = obj.AddComponent<GameEngine>();
                instance.Init();
				DontDestroyOnLoad(obj);
            }

            return instance;
        }
    }

    public AssetBundleMgr BundleMgr
    {
        get { return bundleMgr; }
    }

    public LuaScriptMgr LuaMgr
    {
        get { return luaMgr; }
    }

    void Init()
    {
        bundleMgr = gameObject.AddComponent<AssetBundleMgr>();

        luaMgr = new LuaScriptMgr();
        LuaStatic.Load = Utils.LuaLoader;

        bundleMgr.Init();
        luaMgr.Start();
    }

    void Start()
    {
        Object guanyu = BundleMgr.LoadAsset("Prefabs/guanyu.prefab");
        GameObject.Instantiate(guanyu);

        Object mainCityPanel = BundleMgr.LoadAsset("Prefabs/MainCity.prefab");
        GameObject mainCity = GameObject.Instantiate(mainCityPanel) as GameObject;
        mainCity.name = "MainCity";
        GameObject uiroot = GameObject.Find("UI Root");
        mainCity.transform.SetParent(uiroot.transform);
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

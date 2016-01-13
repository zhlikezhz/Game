#define RESOURCES_DEBUG
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class AssetBundleMgr : MonoBehaviour
{
    public delegate void AssetFunc(Object asset);
    private List<string> loadingList = new List<string>();
    private List<AssetBundleData> bundleData = new List<AssetBundleData>();
    private Dictionary<string, int> assetRef = new Dictionary<string, int>();
    private Dictionary<string, Object> loadedList = new Dictionary<string, Object>();
    private Dictionary<string, AssetBundleData> bundleDependency = new Dictionary<string, AssetBundleData>();

    public void Init()
    {
        Caching.CleanCache();
        InitBundleDependency();
    }

    //外部调用、同步加载资源
    public Object LoadAsset(string path)
    {
        if (isLoadedAsset(path))
        {
            return loadedList[path];
        }

        if (isLoadingAsset(path))
        {
            Debug.LogWarning(string.Format("{0} asset loading!!!", path));
            return null;
        }

        if (bundleDependency.ContainsKey(path))
        {
            List<string> dependencies = bundleDependency[path].dependAssets;
            foreach ( string dependFile in dependencies )
            {
                if (isLoadedAsset(path) == false)
                {
                    Load(dependFile);
                }
                RefAssets(dependFile);
            }
        }

        Load(path);
        RefAssets(path);
        return loadedList[path];
    }

    //外部调用、异步加载资源
    public void LoadAsyncAsset(string path, AssetFunc callback)
    {
        if (isLoadedAsset(path))
        {
            callback(loadedList[path]);
            return;
        }

        if(isLoadingAsset(path))
        {
            Debug.LogWarning(string.Format("{0} asset loading!!!", path));
            return;
        }

        if (bundleDependency.ContainsKey(path))
        {
            List<string> dependencies = bundleDependency[path].dependAssets;
            foreach (string dependFile in dependencies)
            {
                if (isLoadedAsset(path) == false)
                {
                    StartCoroutine(LoadAsync(dependFile));
                }
                RefAssets(dependFile);
            }

        }
        StartCoroutine(LoadAsync(path, callback));
        RefAssets(path);
    }

    public void UnrefAssets(string path)
    {
        if (bundleDependency.ContainsKey(path))
        {
            List<string> dependencies = bundleDependency[path].dependAssets;
            foreach (string dependFile in dependencies)
            {
                if (assetRef.ContainsKey(dependFile))
                {
                    assetRef[dependFile]--;
                }
            }
        }
        if(assetRef.ContainsKey(path))
        {
            assetRef[path]--;
        }
    }

    public void RefAssets(string path)
    {
        if(assetRef.ContainsKey(path))
        {
            assetRef[path]++;
        }
        else
        {
            assetRef[path] = 1;
        }
    }

    public void UnloadUnusedAssets()
    {
        Dictionary<string, int> tmp = new Dictionary<string, int>(assetRef);

        foreach(var item in tmp)
        {
            if(item.Value <= 0)
            {
                assetRef.Remove(item.Key);
                if(loadedList.ContainsKey(item.Key))
                {
                    Resources.UnloadAsset(loadedList[item.Key]);
                    loadedList.Remove(item.Key);
                }
            }
        }

    }

    public bool isLoadedAsset(string path)
    {
        return loadedList.ContainsKey(path);
    }

    private bool isLoadingAsset(string path)
    {
        return loadingList.Contains(path);
    }

    private void Load(string path)
    {
#if RESOURCES_DEBUG
        loadingList.Add(path);
        string fullpath = FullPath(path);
        loadedList[path] = Resources.LoadAssetAtPath(fullpath, typeof(Object));
        loadingList.Remove(path);
#else
        loadingList.Add(path);
        string fullpath = FullPath(path);
        WWW bundle = WWW.LoadFromCacheOrDownload(fullpath, 1);
        AssetBundle asset = bundle.assetBundle;
        loadedList[path] = asset.Load(GetAssetName(path), typeof(Object));
        StartCoroutine(UnloadAssetBundle(asset));
        bundle = null;

        loadingList.Remove(path);
#endif
    }

    private IEnumerator LoadAsync(string path, AssetFunc callback = null)
    {
#if RESOURCES_DEBUG
        loadingList.Add(path);
        string fullpath = FullPath(path);
        Object obj = Resources.LoadAssetAtPath(fullpath, typeof(Object));
        yield return obj;

        loadedList[path] = obj;

        if (callback != null)
        {
            callback(loadedList[path]);
        }

        loadingList.Remove(path);
#else
        loadingList.Add(path);
        string fullpath = FullPath(path);
        WWW bundle = WWW.LoadFromCacheOrDownload(fullpath, 1);
        yield return bundle;

        AssetBundle asset = bundle.assetBundle;
        AssetBundleRequest req = asset.LoadAsync(GetAssetName(path), typeof(Object));
        yield return req;

        loadedList[path] = req.asset;
        StartCoroutine(UnloadAssetBundle(asset));
        bundle = null;

        if (callback != null) 
        {
            callback(loadedList[path]);
        }

        loadingList.Remove(path);
#endif
    }

    private IEnumerator UnloadAssetBundle(AssetBundle bundle)
    {
        yield return new WaitForSeconds(1.0f);
        bundle.Unload(false);
    }

    private string GetAssetName(string name)
    {
        return Path.GetFileNameWithoutExtension(name);
    }

    private string FullPath(string path)
    {
#if RESOURCES_DEBUG
        return "Assets/Build/" + path;
#else
        return Utils.StreamingAssetBundelPath() + "/" + path + ".assetbundle";
#endif
    }

    private void InitBundleDependency()
    {
#if !RESOURCES_DEBUG
        string denpendencyFile = FullPath("assetbundle.txt");
        TextAsset asset = AssetDatabase.LoadAssetAtPath("Assets/StreamingAssets/assetbundle.txt", typeof(TextAsset)) as TextAsset;
        bundleData = LitJson.JsonMapper.ToObject<List<AssetBundleData>>(asset.text);

        foreach(AssetBundleData bundle in bundleData) 
        {
            bundleDependency.Add(bundle.name, bundle);
        }
#endif
    }
}

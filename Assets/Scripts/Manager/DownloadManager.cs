using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class DownloadManager : MonoBehaviour 
{
    public void Init()
    {
        if (GameSetting.isHotUpdate == true)
        {
            Debug.Log(Utils.PresistentDataPath());
            StartCoroutine(HotUpdate());
        }
    }

    private IEnumerator HotUpdate()
    {
        Debug.Log("begin hot update");
        List<BundleData> updateFileList = new List<BundleData>();

        Debug.Log("download version file");
        /////// Download Version File ///////
        string serverVersionFilePath = Utils.HttpDataPath() + "assetbundle.txt";
        WWW serverVersionFile = new WWW(serverVersionFilePath);
        yield return serverVersionFile;
        List<BundleData> serverBundleList = LitJson.JsonMapper.ToObject<List<BundleData>>(serverVersionFile.text);

        string localVersionFilePath = Utils.RealPath("assetbundle.txt");
        StreamReader localVersionFile = File.OpenText(localVersionFilePath);
        List<BundleData> localBundleList = LitJson.JsonMapper.ToObject<List<BundleData>>(localVersionFile.ReadToEnd());
        Dictionary<string, BundleData> localBundleMapping = new Dictionary<string, BundleData>();

        Debug.Log("update file list");
        /////// Get Update File List ///////
        foreach(BundleData bundle in localBundleList)
        {
            localBundleMapping[bundle.name] = bundle;
        }

        foreach(BundleData bundle in serverBundleList)
        {
            if(localBundleMapping.ContainsKey(bundle.name))
            {
                BundleData localBundle = localBundleMapping[bundle.name];
                if(localBundle.md5 != bundle.md5)
                {
                    updateFileList.Add(bundle);
                }
            }
            else
            {
                updateFileList.Add(bundle);
            }
        }


        Debug.Log("download bundle file");
        /////// Download Bundle File ///////
        foreach(BundleData bundle in updateFileList)
        {
            Debug.Log(bundle.name);
            string serverBundlePath = Utils.HttpDataPath() + bundle.name + ".assetbundle";
            WWW serverBundleFile = new WWW(serverBundlePath);
            yield return serverBundleFile;
            Utils.SaveFile(serverBundleFile.bytes, Utils.PresistentDataPath() + bundle.name + ".assetbundle");
            serverBundleFile.Dispose();
        }

        Utils.SaveFile(serverVersionFile.bytes, Utils.PresistentDataPath() + "assetbundle.txt");
        serverVersionFile.Dispose();
    }


}

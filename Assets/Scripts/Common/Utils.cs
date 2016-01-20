using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Utils
{
    static List<string> luaPaths = new List<string> { "Lua/uLua", "Lua" };

    public static List<string> LuaPaths
    {
        get { return luaPaths; }
    }

    public static void AddLuaPath(string path)
    {
        luaPaths.Add(path);
    }

    public static GameManager Engine()
    {
        return GameManager.Instance;
    }

    public static BundleManager BundleMgr()
    {
        return GameManager.Instance.BundleMgr;
    }

    public static byte[] LuaLoader(string filename)
    {
        if (filename.EndsWith(".txt"))
        {
            int index = filename.LastIndexOf('.');
            filename = filename.Substring(0, index);
        }

        if (filename.EndsWith(".lua"))
        {
            int index = filename.LastIndexOf('.');
            filename = filename.Substring(0, index);
        }
        filename = filename.Replace(".", "/");


        byte[] str = null;
        List<string> luaPaths = LuaPaths;
        foreach (string path in luaPaths)
        {
            string fullPath = "";
            if (GameSetting.isEditorModel)
            {
                fullPath = string.Format("{0}/Build/{1}/{2}.lua.txt", Application.dataPath, path, filename);
            }
            else
            {
                fullPath = string.Format("{0}{1}/{2}.lua.txt.assetbundle", StreamingAssetsPath(), path, filename);
            }
            fullPath = fullPath.Replace('\\', '/');

            if (File.Exists(fullPath))
            {
                string assetName = string.Format("{0}/{1}.lua.txt", path, filename);
                TextAsset asset = BundleMgr().LoadAsset(assetName) as TextAsset;
                str = asset.bytes;
                return str;
            }
        }
        return str;
    }

    public static string StreamingAssetsPath()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return Application.dataPath + "/Raw/";
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            return "jar:file://" + Application.dataPath + "!/assets/";
        }
        else
        {
            return Application.dataPath + "/StreamingAssets/";
        }
    }

    public static string LocalAssetBundlePath()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return Application.dataPath + "/Raw/";
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            return "jar:file://" + Application.dataPath + "!/assets/";
        }
        else
        {
            //return "file://" + Application.dataPath + "/StreamingAssets/";
            return Application.dataPath + "/StreamingAssets/";
        }
    }

    public static void CreateDirectories(string path)
    {
        path = path.Replace("\\", "/");
        string[] paths = path.Split(new char[] { '/' });

        string subPath = "";
        for (int i = 0; i < paths.Length; i++)
        {
            subPath = subPath + paths[i] + "/";
            if (Directory.Exists(subPath) == false)
            {
                Directory.CreateDirectory(subPath);
            }
        }
    }

    public static void DoFile(string filename)
    {
        GameManager.Instance.LuaMgr.DoFile(filename);
    }

    public static string MD5(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("MD5() fail,error:" + ex.Message);
        }
    }
}
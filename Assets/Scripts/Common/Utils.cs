﻿using System;
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
                fullPath = string.Format("{0}{1}/{2}.lua.txt.assetbundle", StreamingDataPath(), path, filename);
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

    public static string StreamingDataPath()
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

    public static string PresistentDataPath()
    {
        return Application.persistentDataPath + "/";
    }

    public static string HttpDataPath()
    {
        return "http://192.168.0.150/StreamingAssets/";
    }

    public static string RealPath(string path)
    {
        string realPath = PresistentDataPath() + path;
        if(!File.Exists(realPath))
        {
            realPath = StreamingDataPath() + path;
            if(!File.Exists(realPath))
            {
                Debug.LogError(string.Format("Resource not exist : {0}", path));
            }
        }
        return realPath;
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

    public static void SaveFile(byte[] data, string path)
    {
        string directory = System.IO.Path.GetDirectoryName(path);
        Utils.CreateDirectories(directory);

        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
        stream.Write(data, 0, data.Length);
        stream.Flush();
        stream.Close();
    }

    public static object[] DoFile(string filename)
    {
        return GameManager.Instance.LuaMgr.DoFile(filename);
    }
    
    public static object[] DoString(string filename)
    {
        return GameManager.Instance.LuaMgr.DoString(filename);
    }

    public static string md5file(string fileName)
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
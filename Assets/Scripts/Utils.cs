using System;
using System.IO;
using System.Text; 
using System.Collections;  
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Utils
{
    public static string[] LuaPaths()
    {
        string[] luaPaths = {
                StreamingAssetsPath() + "/Scripts/Lua/uLua/",
                StreamingAssetsPath() + "/Scripts/Lua/",
            };
        return luaPaths;
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
        string[] luaPaths = LuaPaths();
        foreach (string path in luaPaths)
        {
            string fullPath = path + filename + ".lua.txt";
            fullPath = fullPath.Replace('\\', '/');
            if (File.Exists(fullPath))
            {
                str = File.ReadAllBytes(fullPath);
            }
        }
        return str;
    }

    public static string StreamingAssetsPath()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return Application.streamingAssetsPath;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            return Application.streamingAssetsPath;
        }
        else
        {
            return Application.dataPath + "/Build";
        }
    }

    public static string StreamingAssetBundelPath()
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
            return "file://" + Application.dataPath + "/StreamingAssets/";
        }
    }

    public static string PersistentDataPath()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return Application.persistentDataPath;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            return Application.persistentDataPath;
        }
        else
        {
            return Application.persistentDataPath;
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
        GameEngine.Instance.LuaMgr.DoFile(filename);
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
            for (int i = 0; i < retVal.Length; i++) {
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
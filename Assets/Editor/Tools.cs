using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class Tools 
{
    [MenuItem("Tools/convert lua selected to txt", false, 10)]
    public static void ConvertLua2Txt()
    {
        Object[] objs = Selection.objects;
        foreach (Object obj in objs)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (assetPath.EndsWith(".lua"))
            {
                string newAssetPath = assetPath + ".txt";
                AssetDatabase.DeleteAsset(newAssetPath);
                AssetDatabase.CopyAsset(assetPath, newAssetPath);
            }
        }
        AssetDatabase.Refresh();
    }

    /*
    [MenuItem("Tools/convert lua folder to txt", false, 11)]
    public static void ConvertLuaFolder2Txt()
    {
        string path = Application.dataPath + "/Build/Lua";
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (file.EndsWith(".lua"))
                {
                    string assetPath = file.Replace(Application.dataPath, "Assets");
                    string newAssetPath = assetPath + ".txt";
                    AssetDatabase.DeleteAsset(newAssetPath);
                    AssetDatabase.CopyAsset(assetPath, newAssetPath);
                }
            }
            AssetDatabase.Refresh();
        }
    }
    */


    [MenuItem("Tools/Print Dependencies", false, 12)]
    public static void PrintDependencies()
    {
        Object[] objs = Selection.objects;
        foreach (Object obj in objs)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            string[] dependList = AssetDatabase.GetDependencies(new string[] { path });
            foreach (string depend in dependList)
            {
                Debug.Log(string.Format("{0} -> {1}", path, depend));
            }
        }
    }
}

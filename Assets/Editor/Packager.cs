using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Packager {
    static string appPath = Application.dataPath + "/";
    static List<string> buildList = new List<string>();
    static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string,AssetBundle>();
    static Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();

    [MenuItem("Game/Build iPhone Resource", false, 11)]
    public static void BuildiPhoneResource() {
        BuildTarget target;
#if UNITY_5
        target = BuildTarget.iOS;
#else
        target = BuildTarget.iPhone;
#endif
        BuildReources(target, false);
    }

    [MenuItem("Game/Build Android Resource", false, 12)]
    public static void BuildAndroidResource() {
        BuildReources(BuildTarget.Android, true);
    }

    [MenuItem("Game/Build Windows Resource", false, 13)]
    public static void BuildWindowsResource() {
        BuildReources(BuildTarget.StandaloneWindows, true);
    }

    [MenuItem("Game/Print Dependencies", false, 14)]
    public static void PrintDependencies()
    {
        Object[] objs = Selection.objects;
        foreach (Object obj in objs)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            string[] dependList = AssetDatabase.GetDependencies(new string[] { path });
            foreach(string depend in dependList)
            {
                Debug.Log(string.Format("{0} -> {1}", path, depend));
            }
        }
    }


    public static void BuildReources(BuildTarget target, bool isWin)
    {
        buildList.Clear();
        dependencies.Clear();
        assetBundles.Clear();
        string streamPath = appPath + "StreamingAssets/";
        BuildDependenciesFromPath(appPath + "Build/");
        BuildAssetBundleFromDependenices(target);
        //WriteDependencies2Lua(streamPath + "assetbundle.lua");
        WriteDependencies2Json(streamPath + "assetbundle.txt");
        AssetDatabase.Refresh();
    }

    public static void BuildDependenciesFromPath(string basePath)
    {
        List<string> paths = new List<string>();
        paths.Add(basePath);

        string[] directories = Directory.GetDirectories(basePath, "*.*", SearchOption.AllDirectories);
        foreach (string director in directories)
        {
            string newPath = director.Replace("\\", "/");
            string[] folderNames = newPath.Split(new char[] { '/' });
            string folderName = folderNames[folderNames.Length - 1];
            paths.Add(newPath);
        }

        foreach (string path in paths)
        {
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string newPath = file.Replace("\\", "/");
                newPath = "Assets/" + newPath.Replace(appPath, "");
                string guid = AssetDatabase.AssetPathToGUID(newPath);
                string fileExtension = Path.GetExtension(newPath);

                //过滤meta文件、已经被生成过的AB。
                if (fileExtension != ".meta" && dependencies.ContainsKey(guid) == false)
                {
                    List<string> dependencieList = new List<string>();
                    string[] dependencieFileList = AssetDatabase.GetDependencies(new string[] { newPath });
                    foreach (string dependencieFile in dependencieFileList)
                    {
                        string dependencieGuid = AssetDatabase.AssetPathToGUID(dependencieFile);
                        if (guid != dependencieGuid)
                        {
                            dependencieList.Add(dependencieGuid);
                        }
                    }
                    dependencies.Add(guid, dependencieList);
                }
            }
        }
    }

    public static void BuildAssetBundleFromDependenices(BuildTarget target)
    {
        foreach (KeyValuePair<string, List<string>> kvp in dependencies)
        {
            ParseDependenices(kvp.Key, kvp.Value);
        }

        BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies |
                                          BuildAssetBundleOptions.CompleteAssets |
                                          BuildAssetBundleOptions.DeterministicAssetBundle;

        BuildPipeline.PushAssetDependencies();
        foreach (string guid in buildList)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string assetName = Path.GetFileName(assetPath);
            string assetExtension = Path.GetExtension(assetPath);
            string assetDirectory = Path.GetDirectoryName(assetPath);
            string bundlePath = assetDirectory.Replace("Assets/Build", "Assets/StreamingAssets");
            CreateDirectories(bundlePath);
            bundlePath = bundlePath + "/" + assetName + ".assetbundle";
            if (assetExtension == ".prefab") { BuildPipeline.PushAssetDependencies(); }
            Object obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
            BuildPipeline.BuildAssetBundle(obj, null, bundlePath, options, target);
            if (assetExtension == ".prefab") { BuildPipeline.PopAssetDependencies(); }
        }
        BuildPipeline.PopAssetDependencies();
    }

    public static void ParseDependenices(string key, List<string> value)
    {
        if (buildList.Contains(key))
        {
            return;
        }

        string assetFile = AssetDatabase.GUIDToAssetPath(key);
        foreach (string guid in value)
        {
            if (dependencies.ContainsKey(guid))
            {
                ParseDependenices(guid, dependencies[guid]);
            }
            else
            {
                string dependFile = AssetDatabase.GUIDToAssetPath(guid);
                string dependExtension = Path.GetExtension(dependFile);
                if (dependExtension != ".cs")
                {
                    UnityEngine.Debug.LogWarning(string.Format("{0} -> {1} not exist", assetFile, dependFile));
                }
            }
        }

        buildList.Add(key);
    }

    public static void WriteDependencies2Json(string path)
    {
        List<AssetBundleData> datas = new List<AssetBundleData>();
        foreach (KeyValuePair<string, List<string>> kvp in dependencies)
        {
            AssetBundleData data = new AssetBundleData();
            string assetPath = AssetDatabase.GUIDToAssetPath(kvp.Key);
            data.name = assetPath.Replace("Assets/Build/", "");
            data.dependAssets = FindDependencies(kvp.Key);
            datas.Add(data);
        }

        FileStream fs = new FileStream(path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        string jsonStr = JsonFormatter.PrettyPrint(LitJson.JsonMapper.ToJson(datas));
        sw.Write(jsonStr);
        sw.Flush();
        sw.Close();
        fs.Close();
    }

    public static void WriteDependencies2Lua(string path)
    {
        path = path.Replace("/", "\\");
        FileStream fs = new FileStream(path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);

        sw.Write("gdABDependencies = {\n");
        foreach (KeyValuePair<string, List<string>> kvp in dependencies)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(kvp.Key);
            string bundlePath = assetPath.Replace("Assets/Build/", "");
            sw.Write("\t[\"" + bundlePath + "\"] = {\n");

            int count = 1;
            List<string> dependList = FindDependencies(kvp.Key);
            foreach (string depend in dependList)
            {
                sw.Write(string.Format("\t\t[{0}] = \"{1}\",\n", count, depend));
                count++;
            }

            sw.Write("\t},\n");
        }
        sw.Write("}\n");

        sw.Flush();
        sw.Close();
        fs.Close();
    }

    public static List<string> FindDependencies(string key)
    {
        List<string> dependList = new List<string>();
        if (dependencies.ContainsKey(key))
        {
            List<string> guids = dependencies[key];
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string bundlePath = assetPath.Replace("Assets/Build/", "");

                List<string> subDependList = FindDependencies(guid);
                foreach (string subDepend in subDependList)
                {
                    if (!dependList.Contains(subDepend))
                    {
                        dependList.Add(subDepend);
                    }
                }

                if (!dependList.Contains(bundlePath) && dependencies.ContainsKey(guid))
                {
                    dependList.Add(bundlePath);
                }
            }
        }
        return dependList;
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
}


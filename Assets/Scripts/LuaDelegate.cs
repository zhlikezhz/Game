using UnityEngine;
using System.Collections;
using LuaInterface;

public class LuaDelegate : MonoBehaviour
{
    public TextAsset luaFile;
    public LuaTable LuaModule
    {
        get;
        private set;
    }

    void OnEnable()
    {
        if(luaFile == null) 
        {
            Debug.LogError(string.Format("{0}: 没有绑定Lua脚本", gameObject.name));
        }
    }

    void Start()
    {
        RunLuaFile();
        CallLuaFunction("Start", this.LuaModule);
    }

    public void Excute()
    {
        CallLuaFunction("Excute", this.LuaModule);
    }

    void RunLuaFile()
    {
        if (luaFile == null || string.IsNullOrEmpty(luaFile.text))
            return;

        object[] luaRet = GameEngine.Instance.LuaMgr.DoFile(luaFile.name);
        if (luaRet != null && luaRet.Length >= 1)
        {
            this.LuaModule = luaRet[0] as LuaTable;
            this.LuaModule["gameObject"] = this.gameObject;
            this.LuaModule["transform"] = this.transform;
        }
        else
        {
            Debug.LogError("Lua脚本没有返回Table对象：" + luaFile.name);
        }
    }

    //CallLuaFunction("Awake", this.LuaModule, this.gameObject);
    void CallLuaFunction(string funcName, params object[] args)
    {
        if (this.LuaModule == null)
            return;

        LuaFunction func = this.LuaModule[funcName] as LuaFunction;
        if (func != null)
            func.Call(args);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class CopyDLLs
{
    //文件从哪来
    public static string src = "Library/ScriptAssemblies";
    //文件copy到哪里去
    public static string dest = "Assets/CSharpHotFit/AddressableDLL";
    public static string[] files = new[] {"HelloDLL.dll","HelloDLL.pdb"};
    [MenuItem("Tools/Copy DLLs")]
    public static void DoCopyDlls() 
    {
        Directory.CreateDirectory(dest);
        foreach (var f in files)
        {
            //因为Unity禁止代码热更新的, 所以添加后缀名欺骗一手。改名加上.bytes后缀
            Debug.Log($"{Path.Combine(src, f)}=>{Path.Combine(dest, f + ".bytes")}");
            File.Copy(Path.Combine(src,f),Path.Combine(dest,f+".bytes"),true);
        }
        AssetDatabase.Refresh();
    }
}

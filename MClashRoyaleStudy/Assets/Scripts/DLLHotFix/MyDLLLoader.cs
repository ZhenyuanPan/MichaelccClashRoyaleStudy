using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MyDLLLoader : MonoBehaviour
{
    /// <summary>
    /// 加载HotFix过来的DLL(已被处理成二进制资源)
    /// </summary>
    async void Start()
    {
        #region 异步加载二进制数据(DLL&PDB)到内存
        // TextAsset类型用于加载文本文件和二进制文件
        var dll = await Addressables.LoadAssetAsync<TextAsset>("HelloDLL.dll").Task;
        // 不加载PDB文件是不能调试DLL的, 看不到调用堆栈
        var pdb = await Addressables.LoadAssetAsync<TextAsset>("HelloDLL.pdb").Task;
        #endregion

        //利用反射机制载入到mono虚拟机中, Unity的mono虚拟机是用来内部执行c#脚本的
        //需要同时传入DLL和PDB文件, 这样才能在DLL加载的同时才能拥有调试的一些信息文件
        var ass = Assembly.Load(dll.bytes,pdb.bytes);
        //检测是否加载成功 打印所有该dll中的数据类型(结果应该是HelloDLL这个类)
        foreach (var t in ass.GetTypes())
        {
            print(t);
        }
        //反射调用静态方法SayHello
        Type type = ass.GetType("HelloDLL");
        //参数一 调用对象(静态无实例化调用对象所以是null) 参数二 方法的参数列表(SayHello方法无参数所以是null)
        type.GetMethod("SayHello").Invoke(null,null);
    }

}

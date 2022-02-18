using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCUIFramework;
using UnityEngine.AddressableAssets;

public partial class MainPage
{
    public MainPage() : base(UIType.Normal, UIMode.HideOther, UICollider.None) 
    {
        Debug.LogWarning("TODO: 请修改MainPage页面类型等参数, 或注释此行");
    }
    public void OnStart() 
    {
        //KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");
        //加一次就够了
        this.battlebtn.onClick.AddListener(() =>
        {
            //双击tab键 添加事件
            Addressables.LoadSceneAsync("MainScene").Completed += MainPage_Completed;

        });
    }

    private void MainPage_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> obj)
    {
        UIPage.CloseAllPages();
        UIPage._ShowPage<BottomFixPage>(true);
        UIPage._ShowPage<TopFixPage>(true);
    }

    //public void MyEventHandler()
    //{
    //}
    protected override void OnActive()
    {
        UIPage._ShowPage<BottomFixPage>(true);
        UIPage._ShowPage<TopFixPage>(true);
    }
}

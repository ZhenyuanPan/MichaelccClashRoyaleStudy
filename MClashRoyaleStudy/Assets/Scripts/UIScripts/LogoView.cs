using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCUIFramework;
using DG.Tweening;
public partial class LogoPage
{
    private float showSeconds = 1f;
    public LogoPage() : base(UIType.Normal, UIMode.HideOther, UICollider.None) 
    {
        Debug.LogWarning("TODO: 请修改LogoPage页面类型等参数, 或注释此行");
    }
    /// <summary>
    /// OnStart
    /// </summary>
    public void OnStart() 
    {
        //KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");
    }
    //public void MyEventHandler()
    //{
    //}

    protected override void OnActive()
    {
        this.progressSlider.value = 0;
        this.progressSlider.DOValue(1, showSeconds).OnComplete(() =>
        {
            UIPage._ShowPage<MainPage>(true);
        });
    }
}

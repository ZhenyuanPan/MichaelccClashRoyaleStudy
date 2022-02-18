using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCUIFramework;
public partial class TopFixPage
{
    public TopFixPage() : base(UIType.Fixed, UIMode.HideOther, UICollider.None) 
    {
        Debug.LogWarning("TODO: 请修改TopFixPage页面类型等参数, 或注释此行");
    }
    public void OnStart() 
    {
        //KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");
    }
    //public void MyEventHandler()
    //{
    //}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCUIFramework;
using static UnityRoyale.Placeable;
using UnityEngine.AddressableAssets;

public partial class GameOverPage
{
    public GameOverPage() : base(UIType.Normal, UIMode.HideOther, UICollider.None) 
    {
        Debug.LogWarning("TODO: 请修改GameOverPage页面类型等参数, 或注释此行");
    }
    public void OnStart() 
    {
        //KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");
       
        //OnStart 只会在加载时候执行一次, 所以 用来绑定时间
        this.okBtn.onClick.AddListener(()=> {
            //加载完成后再关闭
            UIPage.CloseAllPages();
            Addressables.LoadSceneAsync("GameStartScene");
        });
    }

    //public void MyEventHandler()
    //{
    //}
    /// <summary>
    /// 每次显示页面都需要展示的, 需要在OnActive里面写
    /// </summary>
    protected override void OnActive()
    {
        Faction faction = (Faction)this.Data;
        if (faction == Faction.Opponent)
        {
            this.winnerText.text = "Win";
        }
        else if (faction == Faction.Player)
        {
            this.winnerText.text = "Loss";
        }
    }
}

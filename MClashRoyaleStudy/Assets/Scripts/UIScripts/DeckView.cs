using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCUIFramework;
using UnityEngine.AddressableAssets;

public partial class DeckPage
{
    public DeckPage() : base(UIType.Normal, UIMode.HideOther, UICollider.None) 
    {
        Debug.LogWarning("TODO: 请修改DeckPage页面类型等参数, 或注释此行");
    }
    public void OnStart() 
    {
        //KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");
      
    }
    //public void MyEventHandler()
    //{
    //}
    protected override void OnActive()
    {
        //清空UI下的卡牌,做初始化处理
        for (int i = uitransform.childCount-1; i >= 0; i--)
        {
            if (uitransform.GetChild(i).name == "startPos")
            {
                break;
            }
            //方法函数一般都是静态的, 便于调用
            Addressables.ReleaseInstance(uitransform.GetChild(i).gameObject);
            //Destory函数是静态函数
            //UIRoot.Destroy(uitransform.GetChild(i).gameObject);
        }

        CardManager.instance.canvas = this.uitransform;
        CardManager.instance.startPos = this.startPos;
        CardManager.instance.endPos = this.endPos;
        //获取cardArea下4张卡牌的位置
        for (int i = 0; i < this.cardArea.transform.childCount; i++)
        {
            CardManager.instance.cardsPlaceholder[i] = this.cardArea.transform.GetChild(i);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityRoyale.Placeable;

public class CPU : MonoBehaviour
{
    public float interval = 5; //出牌间隔

    private bool isGameOver = false;

    async void Start()
    {
        //cpu订阅事件, 约定, 事件名称 和 事件处理器名称必须保持一致
        KBEngine.Event.registerOut("OnGameOver",this,"OnGameOver");
        
        await PlayAHand();
    }

    /// <summary>
    /// 注意 事件处理器 必须为public方法, 否者kb反射找不到该方法(它内部反射只找公有成员函数)
    /// 注意参数要一致
    /// </summary>
    public void OnGameOver(Faction faction) 
    {
        Debug.Log($"OnGameOver({faction})");
        isGameOver = true;
    }

    /// <summary>
    /// 出牌
    /// </summary>
    /// <returns></returns> 
    async Task PlayAHand() 
    {
        // 这里出兵的协程中, 每一个异步操作前, 都要加isGameOver判定 
        while (true)
        {
            if (isGameOver)
            {
                break;
            }
            await new WaitForSeconds(interval);
            for (int i = 0; i < 1; i++)
            {
                var cardList = MyCardModel.instance.list;
                //RandomRange左闭右开
                //var cardData = cardList[Random.Range(0, cardList.Count)];
                var cardData = cardList[0];
                if (isGameOver)
                {
                    break;
                }

                await MyCardView.CreatePlaceableAsync(cardData, Faction.Opponent, MyPlaceableMgr.instance.transform, new Vector3(Random.Range(-8f, 8f), 0, Random.Range(2f, 7f)));
            }
        }
    }
}

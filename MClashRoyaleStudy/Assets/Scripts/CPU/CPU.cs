using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityRoyale.Placeable;

public class CPU : MonoBehaviour
{
    public float interval = 5; //出牌间隔
    async void Start()
    {
        //await PlayAHand();
    }

    /// <summary>
    /// 出牌
    /// </summary>
    /// <returns></returns> 
    async Task PlayAHand() 
    {
        while (true)
        {
            await new WaitForSeconds(interval);
            for (int i = 0; i < 25; i++)
            {
                var cardList = MyCardModel.instance.list;
                //RandomRange左闭右开
                var cardData = cardList[Random.Range(0, cardList.Count)];
                await MyCardView.CreatePlaceableAsync(cardData, Faction.Opponent, MyPlaceableMgr.instance.transform, new Vector3(Random.Range(-8f, 8f), 0, Random.Range(2f, 7f)));
            }
        }
    }
}

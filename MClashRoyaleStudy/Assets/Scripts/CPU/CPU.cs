using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityRoyale.Placeable;

public class CPU : MonoBehaviour
{
    public float interval = 5; //出牌间隔
    void Start()
    {
        StartCoroutine(PlayAHand());
    }

    /// <summary>
    /// 出牌
    /// </summary>
    /// <returns></returns> 
    IEnumerator PlayAHand() 
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            var cardList = MyCardModel.instance.list;
            //RandomRange左闭右开
            var cardData = cardList[Random.Range(0, cardList.Count)];
            MyCardView.CreatePlaceable(cardData,Faction.Opponent,MyPlaceableMgr.instance.transform,new Vector3(Random.Range(-8f,8f),0,Random.Range(2f,7f)));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;
    public GameObject cardPrefab;
    //我有四张出牌占位
    public Transform[] cardsPlaceholder = new Transform[4];
    public Transform startPos, endPos;
    private Transform previewCard;
    public Transform canvas;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        canvas.gameObject.SetActive(true);
        StartCoroutine(CreateCardInPreviewArea(.1f));
        print(cardsPlaceholder.Length);
        for (int i = 0; i < cardsPlaceholder.Length; i++)
        {
            StartCoroutine(PromoteFromDeck(i,.4f+i));
            StartCoroutine(CreateCardInPreviewArea(.8f+i));
        }

    }
    public IEnumerator CreateCardInPreviewArea(float delay) 
    {
        yield return new WaitForSeconds(delay);
        print($"addToDeck");
        //range函数左闭右开, 值最大为list.count-1
        int iCard = Random.Range(0,MyCardModel.instance.list.Count);
        MyCard card = MyCardModel.instance.list[iCard];
        GameObject cardPrefab = Resources.Load<GameObject>(card.cardPrefab);
        previewCard = Instantiate(cardPrefab, canvas).transform;
        previewCard.position = startPos.position;
        previewCard.localScale = Vector3.one * .7f;
        previewCard.DOMove(endPos.position,.2f);
        
        //给卡牌view 的数据赋值, 在previewArea(预览区中) 不决定是第几张卡牌, 在出牌区决定是第几张牌,是否准许点击事件.
        previewCard.GetComponent<MyCardView>().data = card;

    }
    public IEnumerator PromoteFromDeck(int i,float delay) 
    {
        yield return new WaitForSeconds(delay);
        print($"PromoteFromDeck");
        previewCard.localScale = Vector3.one;
        previewCard.transform.DOMove(
            cardsPlaceholder[i].position,.2f+.05f*i)
            .OnComplete(
            ()=> {
                //完成卡牌到出牌区动画后给卡牌view层赋值, 并准许其移动
                var mycardview = previewCard.GetComponent<MyCardView>();
                mycardview.isInteraction = true;
                mycardview.index = i;
            });
    }
}

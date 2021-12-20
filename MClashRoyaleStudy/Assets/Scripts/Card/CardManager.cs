using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardManager : MonoBehaviour
{
    public bool readyInteraction = false;
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
        for (int i = 0; i < cardsPlaceholder.Length; i++)
        {
            StartCoroutine(PromoteFromDeck(i, .4f + i));
            StartCoroutine(CreateCardInPreviewArea(.8f + i));
        }
    }
    public IEnumerator CreateCardInPreviewArea(float delay) 
    {
        readyInteraction = false;
        yield return new WaitForSeconds(delay);
        //range函数左闭右开, 值最大为list.count-1
        int iCard = Random.Range(0,MyCardModel.instance.list.Count);
        MyCard card = MyCardModel.instance.list[iCard];
        GameObject cardPrefab = Resources.Load<GameObject>(card.cardPrefab);
        previewCard = Instantiate(cardPrefab, canvas).transform;
        previewCard.position = startPos.position;
        previewCard.localScale = Vector3.one * .7f;
        previewCard.DOMove(endPos.position, .2f);

        //给卡牌view 的数据赋值, 在previewArea(预览区中) 不决定是第几张卡牌, 在出牌区决定是第几张牌,是否准许点击事件.
        previewCard.GetComponent<MyCardView>().data = card;
        if (canvas.childCount == 8)
        {
            readyInteraction = true;
        }
    }

    public IEnumerator PromoteFromDeck(int i,float delay) 
    {
        yield return new WaitForSeconds(delay);
        var mycardview = previewCard.GetComponent<MyCardView>();
        mycardview.index = i;
        //print("mycardviewindex"+mycardview.index);
        previewCard.localScale = Vector3.one;
        previewCard.transform.DOMove(cardsPlaceholder[i].position, .2f + .05f * i);
    }


}

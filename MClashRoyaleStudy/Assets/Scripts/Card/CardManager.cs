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
    List<int> cardPlaces = new List<int> { -1, 0, 1, 2, 3 };
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
    }

    public IEnumerator PromoteFromDeck(int i,float delay) 
    {
        yield return new WaitForSeconds(delay);
        var mycardview = previewCard.GetComponent<MyCardView>();
        mycardview.index = i;
        //print("mycardviewindex"+mycardview.index);
        previewCard.localScale = Vector3.one;
        previewCard.transform.DOMove(
            cardsPlaceholder[i].position,.2f+.05f*i)
            .OnComplete(
            ()=> {
                //完成卡牌到出牌区动画后准许其移动
                mycardview.isInteraction = true;
            });
    }

    IEnumerator CreatCard(int placeIndex,float delay) 
    {
        yield return delay;
        int iCard = Random.Range(0, MyCardModel.instance.list.Count);
        MyCard card = MyCardModel.instance.list[iCard];
        GameObject cardPrefab = Resources.Load<GameObject>(card.cardPrefab);
        previewCard = Instantiate(cardPrefab, canvas).transform;
        previewCard.position = startPos.position;
        previewCard.localScale = Vector3.one * .7f;
        previewCard.GetComponent<MyCardView>().data = card;
        var mycardview = previewCard.GetComponent<MyCardView>();
        mycardview.index = placeIndex;
        if (placeIndex == -1)
        {

            previewCard.transform.DOMove(endPos.position, delay);
        }
        else
        {
            Sequence quence = DOTween.Sequence();
            quence.Append(previewCard.transform.DOMove(endPos.position, delay));
            quence.Append(previewCard.transform.DOScale(Vector3.one, 0.2f));
            quence.Append(previewCard.transform.DOMove(cardsPlaceholder[placeIndex].position, delay));
        }
    }

}

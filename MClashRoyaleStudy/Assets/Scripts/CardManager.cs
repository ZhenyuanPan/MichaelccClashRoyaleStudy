using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform[] cards = new Transform[4];
    public Transform startPos, endPos;
    private Transform previewCard;
    public Transform canvas;
    private void Start()
    {
        canvas.gameObject.SetActive(true);
        StartCoroutine(CreateCardInPreviewArea(.1f));
        print(cards.Length);
        for (int i = 0; i < cards.Length; i++)
        {
            StartCoroutine(PromoteFromDeck(i,.4f+i));
            StartCoroutine(CreateCardInPreviewArea(.8f+i));
        }

    }
    IEnumerator CreateCardInPreviewArea(float delay) 
    {
        yield return new WaitForSeconds(delay);
        print($"addToDeck");
        int iCard = Random.Range(0,MyCardModel.instance.list.Count);
        MyCard card = MyCardModel.instance.list[iCard];
        GameObject cardPrefab = Resources.Load<GameObject>(card.cardPrefab);

        previewCard = Instantiate(cardPrefab, canvas).transform;
        previewCard.position = startPos.position;
        previewCard.localScale = Vector3.one * .7f;
        previewCard.DOMove(endPos.position,.2f);
    }
    IEnumerator PromoteFromDeck(int i,float delay) 
    {
        yield return new WaitForSeconds(delay);
        print($"PromoteFromDeck");
        previewCard.localScale = Vector3.one;
        previewCard.transform.DOMove(cards[i].position,.2f+.05f*i);
    }
}

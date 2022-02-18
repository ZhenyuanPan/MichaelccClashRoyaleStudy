using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using MCUIFramework;
public class CardManager : MonoBehaviour
{
    public bool readyInteraction = false;
    public static CardManager instance;
    public GameObject cardPrefab;
    //我有四张出牌占位
    public Transform[] cardsPlaceholder = new Transform[4];
    public Transform startPos, endPos;
    private GameObject previewCard;
    public Transform canvas;
    private void Awake()
    {
        instance = this;
    }
    private async void Start()
    {
        //加载出牌区ui, 创建卡牌必须在出牌区创建完毕再执行
        //由于await是异步关键字, 放在lambda表达式,lambada表达式必须标记为async方法。（await和async成对出现）
        UIPage._ShowPage<DeckPage>(true,null,async()=>
        {
            await InitCreateCard();
        });
    }

    //初始化发牌方法(异步实现)
    public async Task InitCreateCard() 
    {
        await CreateCardInPreviewAreaAsync(.1f);
        for (int i = 0; i < cardsPlaceholder.Length; i++)
        {
            await PromoteFromDeckAsync(i,1);
            await CreateCardInPreviewAreaAsync(1);
        }
    }

    public async Task CreateCardInPreviewAreaAsync(float delay) 
    {
        readyInteraction = false;
        await new WaitForSeconds(delay);
        //range函数左闭右开, 值最大为list.count-1
        int iCard = UnityEngine.Random.Range(0,MyCardModel.instance.list.Count);
        MyCard card = MyCardModel.instance.list[iCard];
        //使用addressables异步实例化 
        //因为创建了一个Task<GameObject>的人内务对象所以 不能为void类型
        previewCard = await Addressables.InstantiateAsync(card.cardPrefab,canvas).Task;
        previewCard.transform.position = startPos.position;
        previewCard.transform.localScale = Vector3.one * .7f;
        previewCard.transform.DOMove(endPos.position, .2f);

        //给卡牌view 的数据赋值, 在previewArea(预览区中) 不决定是第几张卡牌, 在出牌区决定是第几张牌,是否准许点击事件.
        //TODO 是否能捕获异常
        previewCard.GetComponent<MyCardView>().data = card;
        if (canvas.childCount == 9)
        {
            readyInteraction = true;
        }
    }

    public async Task PromoteFromDeckAsync(int i,float delay) 
    {
        await new WaitForSeconds(delay);
        var mycardview = previewCard.GetComponent<MyCardView>();
        mycardview.index = i;
        previewCard.transform.localScale = Vector3.one;
        previewCard.transform.DOMove(cardsPlaceholder[i].position, .2f + .05f * i);
    }


}

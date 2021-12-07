using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityRoyale.Placeable;
/// <summary>
/// MyCardView V是视图层, 处理显示界面的任务,
/// 1. 要存储我们出牌的是第几张卡
/// 2. 存储该卡的数据
/// 3. 既然是视图层, 还需要完成游戏功能的鼠标事件
/// </summary>
public class MyCardView : MonoBehaviour,IDragHandler,IPointerUpHandler,IPointerDownHandler
{
    public MyCard data;
    public int index = -1;
    public bool isInteraction;
    private Camera mainCam;
    private Transform previewHolder;
    private CanvasGroup canvasGroup;
    private void Start()
    {
        mainCam = Camera.main;
        previewHolder = GameObject.Find("PreviewHolder").transform;
        canvasGroup = transform.GetComponent<CanvasGroup>();
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        if (isInteraction == false || index == -1)
        {
            return;
        }


        //TODO: 为了显示选中卡牌在最上, 则需要把该卡牌放到所有卡牌所在节点的最后一个, 使其在绘制时叠加在其他卡牌的上面
        transform.SetAsLastSibling();
    }
    private bool isDragging; //是否卡牌变兵, bool值默认值是false
    
    
    public void OnDrag(PointerEventData eventData)
    {
        if (isInteraction == false || index == -1)
        {
            return;
        }


        //TODO: 移动卡牌到鼠标位置 transform.parent是canvas, 把当前transfrom pos 设置成当前canvas平面上的鼠标位置
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform,eventData.position,null,out Vector3 posWorld);
        transform.position = posWorld;
        //TODO: 利用射线判断碰到场景属于什么位置
        Ray ray = mainCam.ScreenPointToRay(eventData.position);
        bool hitGround = Physics.Raycast(ray,out RaycastHit hit,float.PositiveInfinity,1<<LayerMask.NameToLayer("PlayingField"));
        //TODO 与游玩区域碰撞时候的处理事件
        if (hitGround)
        {
            previewHolder.transform.position = hit.point;
            //TODO 如果卡牌之前没有被拖拽出来(没有变成小兵)
            if (isDragging == false)
            {
                //TODO 1.隐藏卡牌 2.从卡牌数据数组中找到该张卡牌的数据 3.取出小兵数据 4.取出小兵模型偏移 5.生成该卡牌对应的小兵数组, 并且将其设置为预览用的卡牌, 放到统一的节点下(previewHolder)
                canvasGroup.alpha = 0f;
                for (int i = 0; i < data.placeablesIndices.Length; i++)
                {
                    int unitId = data.placeablesIndices[i];
                    MyPlaceable p = null;
                    for (int j = 0; j < MyPlaceableModel.instance.list.Count; j++)
                    {
                        //卡牌编号 对应 兵种编号 取出兵种数据
                        if (MyPlaceableModel.instance.list[j].id == unitId)
                        {
                            //找到 赋值后 就可以跳出该层循环
                            p = MyPlaceableModel.instance.list[j];
                            break;
                        }
                    }
                    //TODO 取出小兵之间的偏移量(相對於鼠標偏移), 以及其他的属性赋值, 浅拷贝赋值, 对值类型字段操作独立出来, 防止一并修改所有的MyPlaceable对象
                    Vector3 offset = data.relativeOffsets[i];
                    MyPlaceable pClone =  p.Clone();
                    pClone.faction = Faction.Player;
                    //TODO 实例化小兵
                    MyPlaceableMgr.instance.StartCoroutine(ResLoadAsync(pClone,offset));
                }
                isDragging = true;
            }
        }
        else //TODO 鼠标没有命中地面(从地面放回到出牌区) 销毁预览小兵
        {
            if (isDragging)
            {
                for (int i = previewHolder.childCount-1; i >=0; i--)
                {
                    Destroy(previewHolder.GetChild(i).gameObject);
                }
                //重新显示卡牌
                isDragging = false;
                canvasGroup.alpha = 1;

            }
        }
    }

    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isInteraction == false || index == -1)
        {
            return;
        }
        Ray ray = mainCam.ScreenPointToRay(eventData.position);
        bool hitGround = Physics.Raycast(ray,float.PositiveInfinity,1<<LayerMask.NameToLayer("PlayingField"));
        //出牌
        if (hitGround)
        {
            OnCardUsed();
            //销毁打出去的卡牌
            Destroy(this.gameObject);
            CardManager.instance.StartCoroutine(CardManager.instance.PromoteFromDeck(index, .5f));
            CardManager.instance.StartCoroutine(CardManager.instance.CreateCardInPreviewArea(1f));
        }
        else 
        {
            //TODO 放回到出牌区
            transform.DOMove(CardManager.instance.cardsPlaceholder[index].position, .2f);
        }
    }

    private void OnCardUsed() 
    {
        //TODO: 游戏单位放到游戏单位管理器下(MyPlaceableMgr)
        for (int i = previewHolder.childCount-1; i >=0 ; i--)
        {
            var trunit = previewHolder.GetChild(i);
            trunit.SetParent(MyPlaceableMgr.instance.transform,true);
            MyPlaceableMgr.instance.friendlyPlaceablesList.Add(trunit.GetComponent<MyPlaceableView>());
        }
    }

    IEnumerator ResLoadAsync(MyPlaceable myPlaceable, Vector3 offset) 
    {
        var resRequest = Resources.LoadAsync<GameObject>(myPlaceable.associatedPrefab);
        yield return resRequest;
        if (resRequest != null)
        {
            var unit = GameObject.Instantiate(resRequest.asset as GameObject, previewHolder, false);
            unit.transform.localPosition = offset;
            unit.GetComponent<MyPlaceableView>().data = myPlaceable;
        }
    }
}

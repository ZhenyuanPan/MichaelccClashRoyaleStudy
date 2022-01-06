using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityRoyale.Placeable;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;


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
    private Camera mainCam;
    private Transform previewHolder;
    private CanvasGroup canvasGroup;
    private GameObject forbiddenArea;
    private void Start()
    {
        mainCam = Camera.main;
        previewHolder = GameObject.Find("PreviewHolder").transform;
        canvasGroup = transform.GetComponent<CanvasGroup>();
        forbiddenArea = GameObject.Find("ForbiddenArea");
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        if (index == -1 || CardManager.instance.readyInteraction == false)
        {
            return;
        }


        //TODO: 为了显示选中卡牌在最上, 则需要把该卡牌放到所有卡牌所在节点的最后一个, 使其在绘制时叠加在其他卡牌的上面
        transform.SetAsLastSibling();
    }
    private bool isDragging; //是否卡牌变兵, bool值默认值是false
    
    
    public async void OnDrag(PointerEventData eventData)
    {

        if (index == -1 || CardManager.instance.readyInteraction == false)
        {
            return;
        }
        //设置禁止区域显示
        forbiddenArea.GetComponent<MeshRenderer>().enabled = true;

        //TODO: 移动卡牌到鼠标位置 transform.parent是canvas, 把当前transfrom pos 设置成当前canvas平面上的鼠标位置
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform,eventData.position,null,out Vector3 posWorld);
        transform.position = posWorld;
        //TODO: 利用射线判断碰到场景属于什么位置
        Ray ray = mainCam.ScreenPointToRay(eventData.position);
        Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("ForbiddenArea") | 1 << LayerMask.NameToLayer("PlayingField"));
        bool hitGround = false;
        if (hit.collider == null||hit.transform.name == "ForbiddenArea")
        {
            hitGround = false;
        }
        else 
        {
            hitGround = true;
        }
        //TODO 与游玩区域碰撞时候的处理事件
        if (hitGround)
        {
            //调整previewHolder的位置 就是调整所有预览状态小兵的位置, 因为小兵都在 previewHolder下
            previewHolder.transform.position = hit.point;
            //TODO 如果卡牌之前没有被拖拽出来(没有变成小兵)
            if (isDragging == false)
            {
                //TODO 1.隐藏卡牌 2.从卡牌数据数组中找到该张卡牌的数据 3.取出小兵数据 4.取出小兵模型偏移 5.生成该卡牌对应的小兵数组, 并且将其设置为预览用的卡牌, 放到统一的节点下(previewHolder)
                canvasGroup.alpha = 0f;
                #region await 延迟所以会造成逻辑bug, 包裹一层warpper则不会
                //print("执行次序, 我明白为什么了, 因为await会阻塞下面的方法。所以isDragging设置不会被执行，所以会多次执行CreateP");
                //await CreatePlaceableAsync(data, Faction.Player, previewHolder);
                //isDragging = true; 
                #endregion
                #region await要写在最后
                isDragging = true;
                await CreatePlaceableAsync(data, Faction.Player, previewHolder);
                #endregion
            }
        }
        else //TODO 鼠标没有命中地面(从地面放回到出牌区) 销毁预览小兵
        {
            if (isDragging)
            {
                for (int i = previewHolder.childCount-1; i >=0; i--)
                {
                    //Destroy(previewHolder.GetChild(i).gameObject);
                    Addressables.ReleaseInstance(previewHolder.GetChild(i).gameObject);
                }
                //重新显示卡牌
                isDragging = false;
                canvasGroup.alpha = 1;
            }
        }
    }
    /// <summary>
    /// 公共静态方法, 用于根据卡牌数据创建小兵
    /// </summary>
    /// <param name="cardData"></param>
    /// <param name="faction"></param>
    /// <param name="parentTrans"></param>
    /// <param name="worldOffset"></param>
    public static void CreatePlaceable(MyCard cardData,Faction faction,Transform parentTrans = null,Vector3 worldOffset = default)
    {
        for (int i = 0; i < cardData.placeablesIndices.Length; i++)
        {
            int unitId = cardData.placeablesIndices[i];
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
            Vector3 offset = cardData.relativeOffsets[i];
            MyPlaceable pClone = p.Clone();
            pClone.faction = faction;
            //TODO 实例化小兵
            MyPlaceableMgr.instance.StartCoroutine(ResLoadAsyncIEnumerator(pClone, offset,faction,parentTrans, worldOffset));
           
        }
    }

    public static async Task CreatePlaceableAsync(MyCard cardData, Faction faction, Transform parentTrans = null, Vector3 worldOffset = default)
    {
        for (int i = 0; i < cardData.placeablesIndices.Length; i++)
        {
            int unitId = cardData.placeablesIndices[i];
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
            Vector3 offset = cardData.relativeOffsets[i];
            MyPlaceable pClone = p.Clone();
            pClone.faction = faction;
            //TODO 实例化小兵
            await ResLoadAsync(pClone, offset, faction, parentTrans, worldOffset);
        }
    }

    public async void OnPointerUp(PointerEventData eventData)
    {
        forbiddenArea.GetComponent<MeshRenderer>().enabled = false;
        if (index == -1 || CardManager.instance.readyInteraction == false)
        {
            return;
        }
        Ray ray = mainCam.ScreenPointToRay(eventData.position);
        Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("ForbiddenArea") | 1 << LayerMask.NameToLayer("PlayingField"));
        bool hitGround = false;
        if (hit.collider == null || hit.transform.name == "ForbiddenArea")
        {
            hitGround = false;
        }
        else
        {
            hitGround = true;
        }
        //出牌
        if (hitGround)
        {
            OnCardUsed();
            //销毁打出去的卡牌
            //Destroy(this.gameObject);
            Addressables.ReleaseInstance(this.gameObject);
            await CardManager.instance.PromoteFromDeckAsync(index, .5f);
            await CardManager.instance.CreateCardInPreviewAreaAsync(1f);
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
            MyPlaceableMgr.friendlyPlaceablesList.Add(trunit.GetComponent<MyPlaceableView>());
        }
    }

    static async Task ResLoadAsync(MyPlaceable myPlaceable, Vector3 offset, Faction faction, Transform parentTrans = null, Vector3 worldOffset = default) 
    {
        GameObject unit = null;
        if (faction == Faction.Player)
        {
            unit = await Addressables.InstantiateAsync(myPlaceable.associatedPrefab, parentTrans).Task;
        }
        else if (faction == Faction.Opponent)
        {
            unit = await Addressables.InstantiateAsync(myPlaceable.alternatePrefab).Task;
        }
        else
        {
            throw new Exception("创造Placeable无类型判定异常");
        }
        //向量加法
        unit.transform.localPosition = offset + worldOffset;
        unit.GetComponent<MyPlaceableView>().data = myPlaceable;

        //如果是敌方单位直接加到, 因为目前是单机, 直接加到敌方单位列表中
        if (faction == Faction.Opponent)
        {
            MyPlaceableMgr.enemyPlaceablesList.Add(unit.GetComponent<MyPlaceableView>());
        }
    }

    static IEnumerator ResLoadAsyncIEnumerator(MyPlaceable myPlaceable,Vector3 offset, Faction faction,Transform parentTrans = null,Vector3 worldOffset = default) 
    {
        ResourceRequest resRequest = null;
        if (faction == Faction.Player)
        {
            resRequest = Resources.LoadAsync<GameObject>(myPlaceable.associatedPrefab);
        }
        else if(faction == Faction.Opponent)
        {
            resRequest = Resources.LoadAsync<GameObject>(myPlaceable.alternatePrefab);
        }
        else 
        {
            throw new Exception("创造Placeable无类型判定异常");
        }
        yield return resRequest;
        if (resRequest != null)
        {
            GameObject unit = null;
            if (parentTrans == null)
            {
                unit = GameObject.Instantiate(resRequest.asset as GameObject);
            }
            else 
            {
                unit = GameObject.Instantiate(resRequest.asset as GameObject, parentTrans, false);
            }
            //向量加法
            unit.transform.localPosition = offset + worldOffset;
            unit.GetComponent<MyPlaceableView>().data = myPlaceable;

            //如果是敌方单位直接加到, 因为目前是单机, 直接加到敌方单位列表中
            if (faction == Faction.Opponent)
            {
                MyPlaceableMgr.enemyPlaceablesList.Add(unit.GetComponent<MyPlaceableView>());
            }
        }
    }
}

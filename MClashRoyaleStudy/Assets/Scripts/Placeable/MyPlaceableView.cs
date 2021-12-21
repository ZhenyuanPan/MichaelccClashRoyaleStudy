using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityRoyale.Placeable;

//每个小兵身上都挂载的脚本 存放小兵的数据, 和画面的显示
public class MyPlaceableView : MonoBehaviour
{
    public MyPlaceable data; // 游戏单位数据
    public float dissolveDuration = 10f; //死亡溶解总时间
    public float dissolveProgress = 0f; // 死亡溶解当前进度

    private void OnDestroy()
    {
        if (data.faction == Faction.Opponent)
        {
            MyPlaceableMgr.enemyPlaceablesList.Remove(this);
        }
        else if (data.faction == Faction.Player)
        {
            MyPlaceableMgr.friendlyPlaceablesList.Remove(this);
        }
    }
}

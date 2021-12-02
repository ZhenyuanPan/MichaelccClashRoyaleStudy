using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MyPlaceableMgr : MonoBehaviour
{
    public static MyPlaceableMgr instance;

    public Transform trEnemyTower;

    private void Awake()
    {
        instance = this;
    }
    public List<MyPlaceableView> friendlyPlaceable = new List<MyPlaceableView>();
    public List<MyPlaceableView> enemyPlaceable = new List<MyPlaceableView>();



    /*
     *核心战斗逻辑都应在小兵manager中实现
     *TODO: 我们要做什么
        1. 区分游戏角色的状态
    */
    private void Update()
    {
        for (int i = 0; i < friendlyPlaceable.Count; i++)
        {
            /*
             * 用挂在载小兵对象上的myplaceableView脚本(已存储在该mgr->list中)
             * 来获取另一个在小兵对象上挂载的ai脚本组件
             */
            MyPlaceableView placeableView = friendlyPlaceable[i];
            //因为继承自BattleAIBase,所以可以找到挂载的子类脚本组件
            BattleAIBase ai = placeableView.GetComponent<BattleAIBase>();

            /*
             *按照游戏单位当前的状态, 执行状态机;
             *1.执行状态内的动作
             *2.执行状态检测
             *3.执行状态转移
             */
            switch (ai.state)
            {
                case AIState.Idle:
                    {
                        //往目标塔行进
                        NavMeshAgent nav = ai.GetComponent<NavMeshAgent>();
                        nav.enabled = true;
                        nav.destination = trEnemyTower.position;
                        //检测是否有敌人在范围内
                        //有则转移到Seek状态
                    }
                    break;
                case AIState.Seek:
                    {
                        //往敌人方向前进
                        //判断是否进入攻击范围
                        //若是转移到攻击状态
                    }
                    break;
                case AIState.Attack:
                    { 
                        //执行攻击动作
                        //检测敌人是否仍在攻击范围内
                        //若否,切换到IDLE状态
                    
                    }
                    break;
                case AIState.Die:
                    { 
                        
                        
                    }
                    break;
                default:
                    break;
            }
        }
      
    }
}

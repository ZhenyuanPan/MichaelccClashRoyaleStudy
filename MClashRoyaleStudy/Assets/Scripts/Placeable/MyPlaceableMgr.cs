using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityRoyale.Placeable;

public partial class MyPlaceable 
{
    public Faction faction = Faction.None;
    /// <summary>
    /// 浅拷贝一个对象
    /// </summary>
    /// <returns></returns>
    public MyPlaceable Clone() 
    {
        return this.MemberwiseClone() as MyPlaceable;
    }
}

public class MyPlaceableMgr : MonoBehaviour
{
    public static MyPlaceableMgr instance;

    public Transform trEnemyTower;

    private void Awake()
    {
        instance = this;
    }
    public List<MyPlaceableView> friendlyPlaceablesList = new List<MyPlaceableView>();
    public List<MyPlaceableView> enemyPlaceablesList = new List<MyPlaceableView>();

    public MyPlaceableView testMPV = null;
    private void Start()
    {
        enemyPlaceablesList.Add(testMPV);
    }

    /*
     *核心战斗逻辑都应在小兵manager中实现
     *TODO: 我们要做什么
        1. 区分游戏角色的状态
    */
    private void Update()
    {
        for (int i = 0; i < friendlyPlaceablesList.Count; i++)
        {
            /*
             * 用挂在载小兵对象上的myplaceableView脚本(已存储在该mgr->list中)
             * 来获取另一个在小兵对象上挂载的ai脚本组件
             */
            MyPlaceableView placeableView = friendlyPlaceablesList[i];
            MyPlaceable data = placeableView.data;
            //因为继承自BattleAIBase,所以可以找到挂载的子类脚本组件
            BattleAIBase ai = placeableView.GetComponent<BattleAIBase>();
            NavMeshAgent nav = ai.GetComponent<NavMeshAgent>();
            Animator ani = placeableView.GetComponent<Animator>();
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
                        //TODO 找场景内最近的敌人,然后转换状态
                        ai.target = FindNearestEnemy(ai.transform.position,data.faction);

                        if (ai.target != null)
                        {
                            print($"找到最近的角色{ai.target.gameObject.name}");
                            ai.state = AIState.Seek;
                            nav.enabled = true;
                            //切换到行走动画
                            ani.SetBool("IsMoving",true);
                        }
                    }
                    break;
                case AIState.Seek:
                    {
                        //往敌人方向前进
                        nav.destination = ai.target.transform.position;
                        //判断是否进入攻击范围, 若是转移到攻击状态并停止移动
                        if (IsInAttackRange(placeableView.transform.position,ai.target.transform.position,placeableView.data.attackRange))
                        {
                            //停止移动
                            nav.enabled = false;
                            ai.state = AIState.Attack;
                        }
                        
                    }
                    break;
                case AIState.Attack:
                    {
                        //执行攻击动作
                        ani.SetTrigger("Attack");
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

    private bool IsInAttackRange(Vector3 myPos, Vector3 targetPos,float attackRange)
    {
        return Vector3.Distance(myPos, targetPos)< attackRange;
    }

    /// <summary>
    /// 该方法 适用于所有阵营单位, 是单位的通用方法, 玩家找敌人, 敌人找玩家
    /// </summary>
    /// <param name="faction"></param>
    /// <returns></returns>
    private BattleAIBase FindNearestEnemy(Vector3 myPos , Faction faction)
    {
        List<MyPlaceableView> units = faction == Faction.Player ? enemyPlaceablesList : friendlyPlaceablesList;
        float x = float.MaxValue;
        BattleAIBase nearest = null;
        for (int i = units.Count-1; i >= 0; i--)
        {
            float d = Vector3.Distance(myPos,units[i].transform.position);
            if (d < x)
            {
                x = d;
                nearest = units[i].GetComponent<BattleAIBase>();
            }
        }
        return nearest;
    }
}

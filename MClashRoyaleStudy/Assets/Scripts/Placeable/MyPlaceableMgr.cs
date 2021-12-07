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
    //投掷物列表
    public List<MyProjectile> friendlyProjList = new List<MyProjectile>();
    public List<MyProjectile> enemyProjList = new List<MyProjectile>();

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

                        if (ai.target == null)
                        {
                            nav.enabled = false;
                            break;
                        }

                        ai.state = AIState.Seek;
                        nav.enabled = true;
                        //切换到行走动画
                        ani.SetBool("IsMoving", true);
                    }
                    break;
                case AIState.Seek:
                    {
                        //判断是否进入攻击范围, 否定判断, 若不在攻击范围则
                        if (IsInAttackRange(placeableView.transform.position,ai.target.transform.position,placeableView.data.attackRange) == false)
                        {
                            //往敌人方向前进
                            nav.destination = ai.target.transform.position;
                            //别忘了跳出状态机
                            break;
                        }
                        //停止移动 并转到攻击状态
                        ai.state = AIState.Attack;
                        nav.enabled = false;
                    }
                    break;
                case AIState.Attack:
                    {
                        //进到攻击状态一定关闭移动动画
                        ani.SetBool("IsMoving",false);
                        //先去去处理不符合逻辑的条件分支
                        //再判断一次是不是还在攻击范围内? 因为某些敌人跑的快, 如果跑出了我们的攻击范围,就去跳回idle状态。
                        if (IsInAttackRange(placeableView.transform.position, ai.target.transform.position, placeableView.data.attackRange) == false)
                        {
                            ai.state = AIState.Idle;
                            //注意处理完要跳出状态机. 执行下一次循环, 这是一个出口, 代表我们已经走完这一条逻辑了
                            break;
                        }
                        //执行攻击动作: 如果上次攻击时间+攻击间隔 > 当前时间 则不准攻击
                        if (Time.time < ai.lastBlowTime + data.attackRatio)
                        {
                            //这条逻辑走完记得break出状态机
                            break;
                        }
                        //执行攻击动作
                        ani.SetTrigger("Attack");
                        //设置最后一次攻击时间
                        ai.lastBlowTime = Time.time;
                        //攻击伤害结算 --> 移交到UnitAI脚本中的动画事件中执行, 保证打击感
                        //ai.target.GetComponent<MyPlaceableView>().data.hitPoints -= data.damagePerAttack;
                        //血量清零 死亡 --> 移交到UnitAI脚本中的动画事件中执行, 否则会有死亡物体延迟bug
                        //if (ai.target.GetComponent<MyPlaceableView>().data.hitPoints <= 0)
                        //{

                        //    if (ai.target.GetComponent<Animator>()!= null)
                        //    {
                        //        ai.target.GetComponent<Animator>().SetTrigger("IsDead");
                        //    }
                        //    print($"{ai.target.transform.name} is dead");
                        //}

                    }
                    break;
                case AIState.Die:
                    {
                        print(transform.name+"死亡");
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityRoyale.Placeable;


//每个小兵都挂在一个, 控制小兵的逻辑ai脚本, 和动画事件做配合
class UnitAI :BattleAIBase
{
    public GameObject projectile;
    public Transform firePos;
    private MyProjectile myProj;
    public void OnDealDamage() 
    {
        if (this == null || target == null)
        {
            return;
        }
        this.target.GetComponent<MyPlaceableView>().data.hitPoints -= this.GetComponent<MyPlaceableView>().data.damagePerAttack;
        //死亡判定
        if (this.target.GetComponent<MyPlaceableView>().data.hitPoints <= 0)
        {
            TargetIsDie();
        }
        else 
        {
            UnderAttact();
        }
    }

    private void TargetIsDie()
    {
        MyPlaceableMgr.instance.OnEnterDie(this.target);
        //自己与敌人相杀 也需要自己死亡
        if (this.GetComponent<MyPlaceableView>().data.hitPoints <= 0)
        {
            MyPlaceableMgr.instance.OnEnterDie(this);
        }
        else
        {
            this.state = AIState.Idle;
            this.target = null;
        }

    }

    public void OnFireProjectile() 
    {
        //实例化一个投射物
        var go = Instantiate<GameObject>(projectile,firePos.position,Quaternion.identity);//放在手部位置(世界坐标),但是不以手部位父节点(不跟手移动)
        //设置投掷物的发射者
        myProj = go.GetComponent<MyProjectile>();
        myProj.caster = this;
        myProj.target = this.target;
        //投掷物的飞行被MyPlaceableMgr统一管理, 伤害计算也在MyPlaceableMgr中计算
        MyPlaceableMgr.instance.allPlacesProjList.Add(go.GetComponent<MyProjectile>());
    }

    /// <summary>
    /// 用于处理攻击到敌人受伤
    /// </summary>
    public void UnderAttact() 
    {
        if (this.target.GetComponent<Animator>() == null)
        {
            return;
        }
        if (this.target.state != AIState.Die)
        {
            this.target.GetComponent<Animator>().SetTrigger("GetHit");
        }
    }

    
}
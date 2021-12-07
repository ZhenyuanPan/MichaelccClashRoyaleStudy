using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//每个小兵都挂在一个, 控制小兵的逻辑ai脚本, 和动画事件做配合
class UnitAI:BattleAIBase
{
    public GameObject projectile;
    public Transform firePos;
    public void OnDealDamage() 
    {
        print("OnDealDamage");
        this.target.GetComponent<MyPlaceableView>().data.hitPoints -= this.GetComponent<MyPlaceableView>().data.damagePerAttack;
        if (this.target.GetComponent<MyPlaceableView>().data.hitPoints <= 0)
        {
            this.target.GetComponent<MyPlaceableView>().data.hitPoints = 0;
            if (this.target.GetComponent<Animator>() != null)
            {
                this.target.GetComponent<Animator>().SetTrigger("IsDead");
            }
            print($"{this.target.transform.name} is dead");
        }
    }

    public void OnFireProjectile() 
    {
        //实例化一个投射物
        var go = Instantiate<GameObject>(projectile,firePos.position,Quaternion.identity);//放在手部位置(世界坐标),但是不以手部位父节点(不跟手移动)
        //设置投掷物的释放者(用于投掷物命中目标后伤害结算)
        go.GetComponent<MyProjectile>().caster = this;
        //投掷物的飞行被MyPlaceableMgr统一管理
        MyPlaceableMgr.instance.friendlyProjList.Add(go.GetComponent<MyProjectile>());
    }
}
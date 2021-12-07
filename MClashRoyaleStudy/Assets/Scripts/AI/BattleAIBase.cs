using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIState 
{
    Idle,
    Seek,
    Attack,
    Die
}

public  class BattleAIBase : MonoBehaviour
{
    public BattleAIBase target = null;// 用于储存攻击目标

    public AIState state = AIState.Idle;
    public float lastBlowTime = 0f;

    public virtual void OnIdle() { }

    public virtual void OnSeeking() { }

    public virtual void OnAttack() { }
    public virtual void OnDie() { }

}

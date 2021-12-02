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
    public AIState state = AIState.Idle;
    public virtual void OnIdle() { }

    public virtual void OnSeeking() { }

    public virtual void OnAttack() { }
    public virtual void OnDie() { }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyProjectile : MonoBehaviour
{
    public BattleAIBase caster;//投掷物的释放者
    public BattleAIBase target;//投掷物的目标
    public float speed;//飞行速度
}

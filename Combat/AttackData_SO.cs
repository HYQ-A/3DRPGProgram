using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Attack",menuName ="Attack/Attack Data")]
public class AttackData_SO : ScriptableObject
{
    public float attackRange;
    //技能攻击距离
    public float skillRange;
    //技能冷却CD
    public float coolDown;
    //最小伤害
    public float minDamge;
    public float maxDamge;
    //暴击伤害
    public float criticalMultiplier;
    //暴击率
    public float criticalChance;


}

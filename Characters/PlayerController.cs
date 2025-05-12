using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Android;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private GameObject attackTarget;
    private float lastAttackTime;
    private CharacterStats CharacterStats;
    private bool isDead;
    //停止距离
    private float stopDistance;

    private void Awake()
    {
        //获取人物的导航组件
        agent = GetComponent<NavMeshAgent>();
        //获得动画脚本
        anim = GetComponent<Animator>();
        CharacterStats = GetComponent<CharacterStats>();
        stopDistance = agent.stoppingDistance;
    }

    private void OnEnable()
    {
        //为OnMouseClicked事件添加移动到目标位置的函数
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        //移动到攻击目标前
        MouseManager.Instance.onEnemyClicked += EventAttack;
        GameManager.Instance.RigisterPlayer(CharacterStats);
    }

    // Start is called before the first frame update
    void Start()
    {
        SaveManager.Instance.LoadPlayerData();
    }

    private void OnDisable()
    {
        if (!MouseManager.IsInitialized) return;
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.onEnemyClicked -= EventAttack;
    }

    // Update is called once per frame
    void Update()
    {
        if (CharacterStats.CurrentHealth == 0)
            isDead = true;

        //如果死亡就进行广播
        if (isDead)
            GameManager.Instance.NotifyObservers();

        SwitchAnimation();
        //攻击动画时间衰减
        lastAttackTime -= Time.deltaTime;
    }

    /// <summary>
    /// 控制人物动画脚本
    /// </summary>
    private void SwitchAnimation()
    {
        //agent.velocity可以获得人物的速度，通过sqrMagnitude将其转化为浮点数float
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        //避免在攻击时点击地面人物不会移动
        StopAllCoroutines();
        if (isDead) return;

        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if (isDead) return;

        if (target != null)
        {
            attackTarget = target;
            //暴击
            CharacterStats.isCritical = UnityEngine.Random.value < CharacterStats.attackData.criticalChance;
            //开启协程
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;

        agent.stoppingDistance = CharacterStats.attackData.attackRange;

        transform.LookAt(attackTarget.transform);

        //下面一行注释的作用是可以在visualstudio的任务列表里看到待完成的任务
        //TODO: 修改攻击范围参数
        while (Vector3.Distance(transform.position, attackTarget.transform.position) > CharacterStats.attackData.attackRange && attackTarget.gameObject != null)
        {
            //移动到需要攻击的目标前
            agent.destination = attackTarget.transform.position;
            yield return null;
        }
        //停止移动
        agent.isStopped = true;

        //攻击动画间隔
        if (lastAttackTime < 0)
        {
            //暴击
            anim.SetBool("Critical", CharacterStats.isCritical);
            anim.SetTrigger("Attack");
            //重置冷却时间
            lastAttackTime = CharacterStats.attackData.coolDown;
        }
    }

    //Animation Event
    private void Hit()
    {
        if(attackTarget.CompareTag("Attackable"))
        {
            if(attackTarget.GetComponent<Rock>())
            {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(this.transform.forward * 20, ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(CharacterStats, targetStats);
        }
    }

}

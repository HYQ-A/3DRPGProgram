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
    //ֹͣ����
    private float stopDistance;

    private void Awake()
    {
        //��ȡ����ĵ������
        agent = GetComponent<NavMeshAgent>();
        //��ö����ű�
        anim = GetComponent<Animator>();
        CharacterStats = GetComponent<CharacterStats>();
        stopDistance = agent.stoppingDistance;
    }

    private void OnEnable()
    {
        //ΪOnMouseClicked�¼�����ƶ���Ŀ��λ�õĺ���
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        //�ƶ�������Ŀ��ǰ
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

        //��������ͽ��й㲥
        if (isDead)
            GameManager.Instance.NotifyObservers();

        SwitchAnimation();
        //��������ʱ��˥��
        lastAttackTime -= Time.deltaTime;
    }

    /// <summary>
    /// �������ﶯ���ű�
    /// </summary>
    private void SwitchAnimation()
    {
        //agent.velocity���Ի��������ٶȣ�ͨ��sqrMagnitude����ת��Ϊ������float
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        //�����ڹ���ʱ����������ﲻ���ƶ�
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
            //����
            CharacterStats.isCritical = UnityEngine.Random.value < CharacterStats.attackData.criticalChance;
            //����Э��
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;

        agent.stoppingDistance = CharacterStats.attackData.attackRange;

        transform.LookAt(attackTarget.transform);

        //����һ��ע�͵������ǿ�����visualstudio�������б��￴������ɵ�����
        //TODO: �޸Ĺ�����Χ����
        while (Vector3.Distance(transform.position, attackTarget.transform.position) > CharacterStats.attackData.attackRange && attackTarget.gameObject != null)
        {
            //�ƶ�����Ҫ������Ŀ��ǰ
            agent.destination = attackTarget.transform.position;
            yield return null;
        }
        //ֹͣ�ƶ�
        agent.isStopped = true;

        //�����������
        if (lastAttackTime < 0)
        {
            //����
            anim.SetBool("Critical", CharacterStats.isCritical);
            anim.SetTrigger("Attack");
            //������ȴʱ��
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

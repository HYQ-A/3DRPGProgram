using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ����״̬
/// </summary>
public enum EnemyStates
{
    GUARD,//վ׮
    PATROL,//Ѳ��
    CHASE,//׷��
    DEAD
}

//���ض���û��NavMeshAgent���ʱ���Զ����NavMeshAgent���
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour, IEndGameObserver
{
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    private Animator anim;
    protected CharacterStats characterStats;
    private Collider coll;
    
    //��ⷶΧ�İ뾶
    [Header("Basic Settings")]
    public float sightRadius;
    //����Ŀ��
    protected GameObject attackTarget;
    //վ׮״̬
    public bool isGuard;
    //�ٶ�
    private float speed;
    //����ͣ��ʱ��
    public float lookAtTime;
    private float remainLookAtTime;
    //�������ʱ�䣬���ڼ��㹥����ȴ
    private float lastAttackTime;
    //��¼վ׮�ֵĽǶ�λ��
    private Quaternion guardRotation;

    [Header("Patrol State")]
    public float patrolRange;
    //Ѳ�ߵ�λ
    private Vector3 wayPoint;
    //��ʼ�����λ
    private Vector3 guradPos;

    private bool playerDead;

    //bool��϶���
    private bool isWalk;
    private bool isChase;
    private bool isFollow;
    private bool isDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();
        speed = agent.speed;
        guradPos = this.transform.position;
        guardRotation = this.transform.rotation;
        remainLookAtTime = lookAtTime;
    }

    private void Start()
    {
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
        //TODO:�����л����޸ĵ�
        GameManager.Instance.AddObserver(this);
    }
    
    //private void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}
    
    private void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }

    private void Update()
    {
        if (characterStats.CurrentHealth == 0)
            isDead = true;

        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
    }

    /// <summary>
    /// boolֵ����������Ķ������Ʊ�������
    /// </summary>
    private void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Death", isDead);
    }

    private void SwitchStates()
    {
        if (isDead)
            enemyStates = EnemyStates.DEAD;
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
        }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;

                if(transform.position!=guradPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guradPos;

                    if(Vector3.Distance(transform.position, guradPos)<=agent.stoppingDistance)
                    {
                        isWalk = false;
                        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, guardRotation, 0.01f);
                    }

                }

                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;

                if(Vector3.Distance(this.transform.position,wayPoint)<=agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime >= 0)
                        remainLookAtTime -= Time.deltaTime;
                    else
                        GetNewWayPoint();
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }

                break;
            case EnemyStates.CHASE:
                agent.speed = speed;
                isWalk = false;
                isChase = true;

                if(!FoundPlayer())
                {
                    isFollow = false;
                    if (remainLookAtTime >= 0)
                    {
                        agent.destination = this.transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else if (isGuard)
                        enemyStates = EnemyStates.GUARD;
                    else
                        enemyStates = EnemyStates.PATROL;
                }
                else
                {
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }

                if(TargetInAttackRange()|| TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    if(lastAttackTime<0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;
                        //�����ж�
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        //ִ�й���
                        Attack();
                    }
                }

                break;
            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;
                agent.radius = 0;
                Destroy(gameObject, 2f);
                break;
        }
    }

    private void Attack()
    {
        transform.LookAt(attackTarget.transform.position);
        if(TargetInAttackRange())
        {
            //����������
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange())
        {
            //���ܹ�������
            anim.SetTrigger("Skill");
        }
    }

    /// <summary>
    /// Ѳ�ߵ�λ��ȡ
    /// </summary>
    private void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        Vector3 randonPoint = new Vector3(guradPos.x + randomX, this.transform.position.y, guradPos.z + randomZ);

        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randonPoint, out hit, patrolRange, 1) ? hit.position : this.transform.position;

    }

    /// <summary>
    /// ���������ж�
    /// </summary>
    /// <returns></returns>
    private bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(this.gameObject.transform.position, attackTarget.transform.position)<=characterStats.attackData.attackRange;
        else
            return false;
    }

    private bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(this.gameObject.transform.position, attackTarget.transform.position) <= characterStats.attackData.skillRange;
        else
            return false;
    }

    private bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(this.transform.position, sightRadius);

        foreach (var target in colliders)
        {
            if(target.gameObject.CompareTag("Player"))
            {
                attackTarget=target.gameObject;
                return true;
            }
        }

        attackTarget = null;
        return false;
    }

    /// <summary>
    /// Ѳ�߷�Χ��С��Scene���ڵ���ʾ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, sightRadius);
    }

    //Animation Event
    private void Hit()
    {
        if(attackTarget != null&&transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    /// <summary>
    /// �ӿڷ���ʵ��
    /// </summary>
    public void EndNotify()
    {
        //��ȡ����
        //ֹͣ�����ƶ�
        //ֹͣAgent
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;

    }
}

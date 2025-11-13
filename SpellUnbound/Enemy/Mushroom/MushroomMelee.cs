using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomMelee : MushroomBase
{
    [SerializeField] private int m_attackDamage = 15;
    private float m_lastAttackMushTime;
    private float m_attackMushCooldown;

    protected override void Start()
    {
        base.Start();
        m_attackMushCooldown = 6f;
        m_lastAttackMushTime = -7f;
    }

    protected override void Update()
    {
        base.Update();

        if (PlayerStatManager.Instance.DeadPlayer)
        {
            agent.isStopped = true;
            enemyAnimator.SetBool("isChasing", false);
            return;
        }
        CheckDistance();
        Move();
    }

    private void CheckDistance()
    {
        if (isAttacking || isSpawning || currentEnemyState == EnemyState.DEAD ||
            currentEnemyState == EnemyState.STAGGER || currentEnemyState == EnemyState.ICE)
            return;

        float distance = Vector3.Distance(transform.position, m_currentTarget.position);

        if (distance <= m_rangedRange)
        {
            if (Time.time - m_lastAttackMushTime >= m_attackMushCooldown)
            {
                currentEnemyState = EnemyState.ATTACK_MAGIC;
            }
            else
            {
                currentEnemyState = EnemyState.IDLE;
            }
        }
        else if (distance <= m_detectionDistance)
        {
            currentEnemyState = EnemyState.CHASE;
        }
        else
        {
            currentEnemyState = EnemyState.IDLE;
        }
    }

    protected override void Move()
    {
        if (isAttacking || isSpawning || currentEnemyState == EnemyState.DEAD ||
            currentEnemyState == EnemyState.STAGGER || currentEnemyState == EnemyState.ICE)
            return;

        switch (currentEnemyState)
        {
            case EnemyState.IDLE:
                IdleMovement();
                break;
            case EnemyState.CHASE:
                ChaseMovement();
                break;
            case EnemyState.ATTACK_MAGIC:
                Attack();
                break;
        }
    }

    protected override void Attack()
    {
        if (isAttacking || m_currentTarget == null) 
            return;

        StartCoroutine(MeleeAttackCoroutine());
        m_lastAttackMushTime = Time.time;
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        isAttacking = true;
        agent.isStopped = true;
        LookAtTarget();
        enemyAnimator.SetTrigger("AttackMush_Melee");
        yield return new WaitForSeconds(0.5f);

        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward, m_rangedRange);
        
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerStatManager.Instance.TakeDamage(m_attackDamage);
            }
            else if (hit.transform == defenseCrystal)
            {
                DefenseCrystal crystal = defenseCrystal.GetComponent<DefenseCrystal>();
                if (crystal != null)
                {
                    crystal.TakeDamage(m_attackDamage);
                }
            }
        }

        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
        agent.isStopped = true;
    }

    protected override void Die(SkillType type, bool isTripleChain)
    {
        base.Die(type, isTripleChain);
        MushroomBoss mushroomBoss = FindObjectOfType<MushroomBoss>();
        if (mushroomBoss != null)
        {
            mushroomBoss.RemoveMushroomList(gameObject);
        }
    }

    private void IdleMovement()
    {
        enemyAnimator.SetBool("isChasing", false);
        agent.isStopped = true;
    }

    private void ChaseMovement()
    {
        agent.isStopped = false;
        agent.SetDestination(m_currentTarget.position);
        enemyAnimator.SetBool("isChasing", true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, m_rangedRange);
    }
}

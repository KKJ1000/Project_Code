using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class LivingArmor : BaseMonster
{
    [Header("[범위 설정]")]
    [SerializeField, Tooltip("탐지 범위")] private float m_detectionRange = 15f;

    [Header("[첫 번째 공격 패턴]")]
    [SerializeField, Tooltip("근접 공격 범위")] private float m_attackRange = 2f;
    [SerializeField, Tooltip("근접 공격 데미지")] private int m_meleeDamage = 15;

    [Header("[두 번째 공격 패턴]")]
    [SerializeField, Tooltip("공격 범위")] private float m_stompRange = 8f;
    [SerializeField, Tooltip("공격 데미지")] private int m_stompDamage = 10;
    [SerializeField, Tooltip("공격 범위 각도")] private float m_stompAngle = 120f;
    [SerializeField, Tooltip("공격 딜레이 시간")] private float m_stompCastTime = 1.5f;

    [Header("[두 번째 공격 이펙트]")]
    [SerializeField] private GameObject m_attackEffectPrefab;

    [Header("[공격 후 쿨타임]")]
    [SerializeField] private float m_attackCooldown = 3.0f;

    [Header("[검 오브젝트")]
    [SerializeField] private GameObject m_swordObject;

    private bool isAttackCooldown = false;
    private bool isAttacking = false;

    protected override void Start()
    {
        base.Start();
        isLivingArmor = true;
        currentEnemyState = EnemyState.IDLE;
    }

    void Update()
    {
        if (_isDead || isSpawning || isAttacking || IsMovementBlocked()) return;

        DistanceChangeState();
        Move();
        LookAtTarget();
    }

    private void DistanceChangeState()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (CanAttack())
        {
            if (distance <= m_attackRange)
            {
                currentEnemyState = EnemyState.ATTACK_MELEE;
            }
            else if (distance <= m_stompRange)
            {
                currentEnemyState = EnemyState.ATTACK_MAGIC;
            }
            else if (distance <= m_detectionRange)
            {
                currentEnemyState = EnemyState.CHASE;
            }
            else
            {
                currentEnemyState = EnemyState.IDLE;
            }
        }
        else
        {
            if (distance < m_attackRange)
            {
                currentEnemyState = EnemyState.IDLE;
            }
            else if (distance <= m_detectionRange)
            {
                currentEnemyState = EnemyState.CHASE;
            }
            else
            {
                currentEnemyState = EnemyState.IDLE;
            }
        }
    }

    protected override void Move()
    {
        switch (currentEnemyState)
        {
            case EnemyState.IDLE:
                agent.isStopped = true;
                enemyAnimator.SetBool("isChasing", false);
                break;
            case EnemyState.CHASE:
                agent.isStopped = false;
                agent.SetDestination(player.position);
                enemyAnimator.SetBool("isChasing", true);
                break;
            case EnemyState.ATTACK_MELEE:
                if (!isAttacking)
                    StartCoroutine(MeleeAttack());
                break;
            case EnemyState.ATTACK_MAGIC:
                if (!isAttacking)
                    StartCoroutine(StompAttack());
                break;
        }
    }

    private IEnumerator MeleeAttack()
    {
        isAttacking = true;
        agent.isStopped = true;
        enemyAnimator.SetTrigger("isAttack_1");

        yield return new WaitForSeconds(1f);
        if (_isDead) yield break;
        if (Vector3.Distance(transform.position, player.position) <= m_attackRange)
        {
            PlayerStatManager.Instance.TakeDamage(m_meleeDamage);
        }
        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
        agent.isStopped = false;
        StartCoroutine(StartAttackCooldown());
    }

    private IEnumerator StompAttack()
    {
        isAttacking = true;
        agent.isStopped = true;
        enemyAnimator.SetTrigger("isAttack_2");
        yield return new WaitForSeconds(1.8f); 

        GameObject effect = Instantiate(m_attackEffectPrefab, transform.position, transform.rotation);

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);
        float angleToPlayer = Vector3.Angle(transform.forward, direction.normalized);

        if (distanceToPlayer <= m_stompRange && angleToPlayer <= m_stompAngle * 0.5f)
        {
            PlayerStatManager.Instance.TakeDamage(m_stompDamage);
        }

        if (_isDead) yield break;
        yield return new WaitForSeconds(m_stompCastTime);
        Destroy(effect);
        isAttacking = false;
        agent.isStopped = false;
        StartCoroutine(StartAttackCooldown());
    }

    protected override void Die(SkillType type, bool isTripleChain)
    {
        if (_isDead) return;
        if (m_swordObject != null)
        {
            m_swordObject.GetComponent<Rigidbody>().isKinematic = false;
        }
        base.Die(type, isTripleChain);
        enemyAnimator.SetBool("isChasing", false);
    }

    private void LookAtTarget()
    {
        bool isInDetectionRange = Vector3.Distance(transform.position, player.position) <= m_detectionRange;

        if (!isAttacking && !IsMovementBlocked() && isInDetectionRange)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;

            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private IEnumerator StartAttackCooldown()
    {
        isAttackCooldown = true;
        yield return new WaitForSeconds(m_attackCooldown);
        isAttackCooldown = false;
    }

    private bool IsMovementBlocked()
    {
        return currentEnemyState == EnemyState.DEAD || currentEnemyState == EnemyState.STAGGER || currentEnemyState == EnemyState.ICE;
    }

    private bool CanAttack()
    {
        return !isAttacking && !isAttackCooldown && !IsMovementBlocked();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_attackRange);
    }
}

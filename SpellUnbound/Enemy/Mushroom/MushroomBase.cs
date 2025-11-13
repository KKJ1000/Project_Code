using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 버섯 몬스터 공통 로직 클래스
/// </summary>
public class MushroomBase : BaseMonster
{
    [Header("탐지 거리")]
    [SerializeField] protected float m_detectionDistance;
    [Header("공격 범위")]
    [SerializeField] protected float m_rangedRange;

    protected EnemyState m_previousState;
    protected Transform m_currentTarget;
    protected bool isAttacking = false;

    protected override void Start()
    {
        base.Start();
        if (spawnArea != null)
        {
            spawnArea.position = transform.position;
        }
    }

    protected virtual void Update()
    {
        if (isSpawning || currentEnemyState == EnemyState.DEAD || currentEnemyState == EnemyState.STAGGER || currentEnemyState == EnemyState.ICE)
            return;

        if (PlayerStatManager.Instance.DeadPlayer)
        {
            agent.isStopped = true;
            enemyAnimator.SetBool("isChasing", false);
            return;
        }

        TargetUpdate();
    }

    public override void Stagger(float duration)
    {
        if (healthPoint > 0)
        {
            if (currentEnemyState != EnemyState.STAGGER)
            {
                m_previousState = currentEnemyState;
            }
            StartCoroutine(StaggerCoroutine(duration));
        }
    }

    private IEnumerator StaggerCoroutine(float duration)
    {
        currentEnemyState = EnemyState.STAGGER;
        agent.isStopped = true;
        enemyAnimator.speed = 0f;

        yield return new WaitForSeconds(duration);

        if (currentEnemyState == EnemyState.DEAD)
            yield break;

        agent.isStopped = false;
        enemyAnimator.speed = 1f;
        currentEnemyState = m_previousState;
    }

    private void TargetUpdate()
    {
        float playerDistance = Vector3.Distance(transform.position, player.position);
        float crystalDistance = float.MaxValue;

        if (defenseCrystal != null)
        {
            crystalDistance = Vector3.Distance(transform.position, defenseCrystal.position);
        }

        m_currentTarget = (playerDistance <= crystalDistance) ? player : defenseCrystal;
    }

    protected IEnumerator AttackAnimation(string trigger, float duration)
    {
        agent.isStopped = true;
        enemyAnimator.SetTrigger(trigger);
        transform.LookAt(m_currentTarget.position);
        yield return new WaitForSeconds(duration);
    }

    protected void LookAtTarget()
    {
        if (!isAttacking && m_currentTarget != null)
        {
            Vector3 direction = m_currentTarget.position - transform.position;
            direction.y = 0f;

            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_detectionDistance);
    }
}

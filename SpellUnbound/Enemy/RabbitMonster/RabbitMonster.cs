using System.Collections;
using UnityEngine;

public class RabbitMonster : BaseMonster
{
    [Header("탐지/공격 범위")]
    [SerializeField, Tooltip("탐지 범위")] protected float detectionRange;
    [SerializeField, Tooltip("공격 범위")] protected float rangedRange;

    [Header("발사 위치")]
    [SerializeField] protected Transform firePos;

    [Header("공격 쿨타임")]
    [SerializeField] protected float attackCooldown;
    protected float lastAttackTime;

    protected bool isAttacking = false;
    protected bool isCharging = false;

    protected Quaternion initialRotation;
    private EnemyState previousState;
    protected Transform currentTarget;

    protected override void Start()
    {
        base.Start();
        if (spawnArea != null)
        {
            transform.position = spawnArea.position;
        }
        initialRotation = transform.rotation;
        lastAttackTime = -8f;
    }

    protected virtual void Update()
    {
        if (currentEnemyState == EnemyState.DEAD || currentEnemyState == EnemyState.STAGGER || currentEnemyState == EnemyState.ICE)
            return;

        if (PlayerStatManager.Instance.DeadPlayer)
        {
            agent.isStopped = true;
            enemyAnimator.SetBool("isChasing", false);
            return;
        }

        TargetUpdate();
    }

    private void TargetUpdate()
    {
        float playerDistance = Vector3.Distance(transform.position, player.position);
        float crystalDistance = float.MaxValue;

        if (defenseCrystal != null)
        {
            crystalDistance = Vector3.Distance(transform.position, defenseCrystal.position);
        }

        currentTarget = (playerDistance <= crystalDistance) ? player : defenseCrystal;
    }

    protected void MoveToDestination(Vector3 targetPos)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh || currentEnemyState == EnemyState.DEAD)
        {
            return;
        }

        enemyAnimator.SetBool("isChasing", true);
        agent.isStopped = false;
        agent.SetDestination(targetPos);
    }

    public override void Stagger(float duration)
    {
        if (healthPoint > 0)
        {
            if (currentEnemyState != EnemyState.STAGGER)
            {
                previousState = currentEnemyState;
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

        currentEnemyState = previousState;
    }

    protected void MoveToStop()
    {
        agent.isStopped = true;
        enemyAnimator.SetBool("isChasing", false);
    }

    protected void TransitionToState(EnemyState chanageState)
    {
        currentEnemyState = chanageState;
    }

    protected bool IsTargetInAttackRange()
    {
        return Vector3.Distance(transform.position, currentTarget.position) <= rangedRange;
    }

    protected bool IsTargetInDetectionRange()
    {
        return Vector3.Distance(transform.position, currentTarget.position) <= detectionRange;
    }

    protected void ChargeLookAtTarget()
    {
        if (!isCharging || currentEnemyState == EnemyState.DEAD || currentEnemyState == EnemyState.STAGGER || currentEnemyState == EnemyState.ICE) return;

        Vector3 targetPos = currentTarget.position + Vector3.up * 0.5f;
        transform.LookAt(targetPos);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
    }
}
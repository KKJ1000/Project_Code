using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueRabbit : RabbitMonster
{
    [Header("파란토끼 몬스터 설정")]
    [SerializeField, Tooltip("캐스팅 이펙트")] private GameObject castingPrefab;
    [SerializeField, Tooltip("발사체 이펙트")] private GameObject projectilePrefab;
    [SerializeField, Tooltip("피격 이펙트")] private GameObject hitPrefab;
    [Space(10)]
    [SerializeField, Tooltip("발사체 데미지")] private int damage_01;
    [SerializeField, Tooltip("발사체 속도")] private float speed_01;
    [SerializeField, Tooltip("발사체 최대 거리")] private int maxDistance_01;

    private GameObject currentCastingEffect;
    private float m_attackHeight;

    protected override void Start()
    {
        base.Start();
        currentEnemyState = EnemyState.IDLE;
    }

    protected override void Update()
    {
        if (isSpawning) return;
        base.Update();
        CheckDistanceAndChageState();
        Move();
        CastingEffectDestroy();
        ChargeLookAtTarget();
    }

    void CheckDistanceAndChageState()
    {
        if (isAttacking || currentEnemyState == EnemyState.DEAD) return;

        if (IsTargetInAttackRange())
        {
            if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
            {
                TransitionToState(EnemyState.ATTACK_MAGIC);
                StartCoroutine(ExecuteAttack());
            }
            else
            {
                TransitionToState(EnemyState.IDLE);
            }
        }
        else if (IsTargetInDetectionRange())
        {
            TransitionToState(EnemyState.CHASE);
            MoveToDestination(currentTarget.position);
        }
        else if (IsPlayerOutOfRange())
        {
            TransitionToState(EnemyState.RETURN);
        }
    }

    protected override void Move()
    {
        if (currentEnemyState == EnemyState.DEAD || currentEnemyState == EnemyState.STAGGER || currentEnemyState == EnemyState.ICE)
            return;

        switch (currentEnemyState)
        {
            case EnemyState.IDLE:
                MoveToStop();
                break;
            case EnemyState.CHASE:
                MoveToDestination(currentTarget.position);
                break;
            case EnemyState.ATTACK_MAGIC:
                MoveToStop();
                break;
            case EnemyState.RETURN:
                MoveToReturn();
                break;
        }
    }

    private IEnumerator ExecuteAttack()
    {
        isAttacking = true;
        agent.isStopped = true;
        isCharging = true;

        enemyAnimator.SetTrigger("AttackHorn_01");
        currentCastingEffect = Instantiate(castingPrefab, firePos.position, firePos.rotation);
        isCharging = true;
        currentCastingEffect.transform.parent = transform;
        yield return new WaitForSeconds(1.7f);
        Destroy(currentCastingEffect);
        currentCastingEffect = null;
        isCharging = false;

        if (currentEnemyState == EnemyState.DEAD) yield break;
        GameObject projectile = Instantiate(projectilePrefab, firePos.position, firePos.rotation);
        if (currentTarget == defenseCrystal)
            m_attackHeight = 1f;
        else
            m_attackHeight = 0.2f;

        Vector3 targetPos = currentTarget.position + Vector3.up * m_attackHeight;
        Vector3 direction = targetPos - firePos.position;
        projectile.transform.forward = direction;
        Projectile projectileCtrl = projectile.GetComponent<Projectile>();
        projectileCtrl.Init(firePos.position, null, hitPrefab);
        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
        lastAttackTime = Time.time;
    }

    private bool IsPlayerOutOfRange()
    {
        return currentEnemyState != EnemyState.IDLE;
    }

    protected void MoveToReturn()
    {
        if (spawnArea != null)
        {
            if (Vector3.Distance(transform.position, spawnArea.position) > agent.stoppingDistance)
            {
                MoveToDestination(spawnArea.position);
            }
            else
            {
                TransitionToState(EnemyState.IDLE);
                enemyAnimator.SetBool("isChasing", false);
                transform.position = spawnArea.position;
                transform.rotation = initialRotation;
            }
        }
        else
        {
            TransitionToState(EnemyState.IDLE);
        }

    }

    private void CastingEffectDestroy()
    {
        if (currentEnemyState == EnemyState.DEAD && currentCastingEffect != null)
        {
            Destroy(currentCastingEffect);
            currentCastingEffect = null;
        }
    }
}

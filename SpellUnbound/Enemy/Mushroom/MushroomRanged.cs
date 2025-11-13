using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomRanged : MushroomBase
{
    [Header("원거리 공격 발사 위치")]
    [SerializeField] protected Transform m_firePosition;
    [Header("원거리 공격 이펙트")]
    [SerializeField] private GameObject m_castEffectPrefab;
    [SerializeField] private GameObject m_projectileEffectPrefab;
    [SerializeField] private GameObject m_hitEffectPrefab;
    [Header("원거리 공격 설정")]
    [SerializeField, Tooltip("발사체 속도")] private float m_attackMushSpeed;
    [SerializeField, Tooltip("발사체 최대 거리")] private float m_attackMushMaxDistance;
    [SerializeField, Tooltip("발사체 데미지")] private int m_attackMushDamage;
    private int m_projectileCount = 3;
    private float m_attackMushCooldown;
    private float m_lastAttackMushTime;

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
        Attack();
        LookAtTarget();
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
        else if (currentEnemyState != EnemyState.PATROL)
        {
            currentEnemyState = (spawnArea != null) ? EnemyState.RETURN : EnemyState.IDLE;
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
            case EnemyState.RETURN:
                ReturnMovement();
                break;
        }
    }

    protected override void Attack()
    {
        if (isAttacking || isSpawning || currentEnemyState == EnemyState.DEAD ||
            currentEnemyState == EnemyState.STAGGER || currentEnemyState == EnemyState.ICE || currentEnemyState != EnemyState.ATTACK_MAGIC)
            return;

        StartCoroutine(ShootProjectile());
        m_lastAttackMushTime = Time.time;
    }

    IEnumerator ShootProjectile()
    {
        StartCoroutine(AttackAnimation("AttackMush_Ranged", m_attackMushCooldown));
        isAttacking = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(0.1f);

        GameObject castEffect = Instantiate(m_castEffectPrefab, m_firePosition.position, Quaternion.identity);
        Vector3 target = m_currentTarget.position;
        if (m_currentTarget == defenseCrystal)
        {
            target += Vector3.up * 1f;
        }
        else
        {
            target += Vector3.up * 0.2f;
        }

        castEffect.transform.SetParent(m_firePosition);
        yield return new WaitForSeconds(1.2f);
        Destroy(castEffect);

        for (int i = 0; i < m_projectileCount; i++)
        {
            GameObject projectile = Instantiate(m_projectileEffectPrefab, m_firePosition.position, m_firePosition.rotation);
            Projectile projectileCtrl = projectile.GetComponent<Projectile>();
            projectileCtrl.Init(m_firePosition.position, null, m_hitEffectPrefab);
            Vector3 direction = target - m_firePosition.position;
            projectile.transform.forward = direction.normalized;
            yield return new WaitForSeconds(0.15f);
        }

        isAttacking = false;
        agent.isStopped = false;
        yield return null;
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

    private void ReturnMovement()
    {
        if (currentEnemyState != EnemyState.RETURN) 
            return;
        if (spawnArea == null) 
            return;

        enemyAnimator.SetBool("isChasing", true);
        bool canReturn = Vector3.Distance(transform.position, spawnArea.position) > agent.stoppingDistance;

        if (canReturn)
        {
            agent.isStopped = false;
            agent.SetDestination(spawnArea.position);
        }
        else
        {
            currentEnemyState = EnemyState.IDLE;
        }
    }

}

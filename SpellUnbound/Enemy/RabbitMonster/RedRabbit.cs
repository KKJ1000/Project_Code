using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedRabbit : RabbitMonster
{
    [Header("빨간토끼 몬스터 설정")]
    [SerializeField, Tooltip("발사체 이펙트")] private GameObject _projectilePrefab;
    [SerializeField, Tooltip("폭발 범위 이펙트")] private GameObject _exploreRangeEffect;
    [SerializeField, Tooltip("폭발 이펙트")] private GameObject _hitPrefab;
    [Space(5)]
    [SerializeField, Tooltip("발사체 속도")] private float _speed;
    [SerializeField, Tooltip("발사체 각도")] private float _angle;

    [Header("순찰 지점")]
    [SerializeField] private Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    protected override void Start()
    {
        base.Start();
        if (spawnArea != null)
        {
            spawnArea = transform;
        }
        if (patrolPoints != null)
        {
            TransitionToState(EnemyState.PATROL);
        }
        else
        {
            TransitionToState(EnemyState.IDLE);
        }
    }

    protected override void Update()
    {
        if (isSpawning) return;
        base.Update();
        ChargeLookAtTarget();
        CheckDistanceAndChageState();
        Move();
    }

    void CheckDistanceAndChageState()
    {
        if (currentEnemyState == EnemyState.DEAD || currentEnemyState == EnemyState.STAGGER || currentEnemyState == EnemyState.ICE || isAttacking) 
            return;

        if (IsTargetInAttackRange())
        {
            if (Time.time >= lastAttackTime + attackCooldown)
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
        else if (IsTargetOutOfRange())
        {
            TransitionToState(EnemyState.IDLE);
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
            case EnemyState.PATROL:
                PatrolMovement();
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
        isCharging = true;
        isAttacking = true;
        agent.isStopped = true;

        enemyAnimator.SetTrigger("AttackHorn_00");
        yield return new WaitForSeconds(0.6f);
        if (currentEnemyState == EnemyState.DEAD) yield break;
        Quaternion originalRotation = firePos.rotation;
        isCharging = false;

        for (int i = 0; i < 3; i++)
        {
            Quaternion quaternion = Quaternion.Euler(_angle, firePos.eulerAngles.y, firePos.eulerAngles.z);
            Vector3 startPosition = firePos.position;

            if (i == 1)
            {
                quaternion = Quaternion.Euler(_angle, firePos.eulerAngles.y + 20f, firePos.eulerAngles.z);
                startPosition += transform.right * 0.3f;
            }
            else if (i == 2)
            {
                quaternion = Quaternion.Euler(_angle, firePos.eulerAngles.y - 20f, firePos.eulerAngles.z);
                startPosition += transform.right * -0.3f;
            }

            GameObject projectile = Instantiate(_projectilePrefab, startPosition, quaternion);
            Projectile projectileCtrl = projectile.GetComponent<Projectile>();
            projectileCtrl.Init(firePos.position, _exploreRangeEffect, _hitPrefab);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            Vector3 force = projectile.transform.forward * _speed;
            rb.AddForce(force, ForceMode.Impulse);
        }

        firePos.rotation = originalRotation;
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        lastAttackTime = Time.time;
    }

    private bool IsTargetOutOfRange()
    {
        return currentEnemyState != EnemyState.PATROL;
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
                TransitionToState(EnemyState.PATROL);
            }
        }
        else
        {
            TransitionToState(EnemyState.IDLE);
        }
    }

    protected void PatrolMovement()
    {
        if (patrolPoints.Length == 0) return;

        if (Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex].position) > agent.stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            enemyAnimator.SetBool("isChasing", true);
        }
        else
        {
            enemyAnimator.SetBool("isChasing", false);
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }
}

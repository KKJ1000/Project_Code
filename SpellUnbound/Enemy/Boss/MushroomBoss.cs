using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MushroomBoss : BaseMonster
{
    [Header("첫번째 공격 패턴설정")]
    [SerializeField] private GameObject m_projectilePrefab01;
    [SerializeField] private float m_projectileSpeed01;
    [SerializeField] private float m_projectileDamage01;
    [SerializeField] private Transform m_firePosition01;

    [Header("두번째 공격 패턴설정")]
    [SerializeField] private GameObject m_projectilePrefab02;
    [SerializeField] private GameObject m_hitEffectPrefab02;
    [SerializeField] private float m_projectileSpeed02;
    [SerializeField] private float m_projectileDamage02;
    [SerializeField] private Transform[] m_firePositions02;

    [Header("2페이즈 일반 몬스터(버섯) 프리팹")]
    [SerializeField] private GameObject m_mushroomPrefab;

    [Header("일반 몬스터(버섯) 소환 위치")]
    [SerializeField] private Transform[] m_spawnPoints;

    [Header("그로기 상태 관리")]
    [SerializeField] private float m_groggyDuration = 3f;
    private int m_attackCount = 0;
    private bool isGroggy = false;

    [Header("보스 정보 UI")]
    [SerializeField] private Text m_bossNameUI;

    [Header("보스 스테이지 시작 거리")]
    [SerializeField] private float startDistance = 20f;

    [Header("몬스터 한 마리당 회복량")]
    [SerializeField] private float m_healPerMushroom = 50f;

    [Header("몬스터 제거 체크 타이머")]
    [SerializeField] private float m_killTimeLimt = 10f; // 일반 몬스터 소환 후 이 시간동안 잡지 못하면 보스 몬스터 체력 회복

    [SerializeField] private BossResultUI bossResultUI;

    MonsterHealthUI m_monsterHealthUI;
    // 보스 스테이지 관리
    private List<GameObject> m_aliveMonsters = new List<GameObject>();
    private float m_maxHealth;
    private int m_currentPhase;
    private int m_lastPattern = 1;
    private bool m_isBossStageInProgress;
    private bool isAttacking = false;

    private bool hasSpawnedAt70 = false;
    private bool hasSpawnedAt40 = false;

    private void Awake()
    {
        IsBoss = true;
        m_isBossStageInProgress = false;
        if (m_bossNameUI != null)
        {
            m_bossNameUI.gameObject.SetActive(false);
        }
        m_maxHealth = healthPoint;
        m_monsterHealthUI = GetComponent<MonsterHealthUI>();
    }

    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        if (PlayerStatManager.Instance.DeadPlayer)
        {
            // 플레이어가 죽으면 보스 공격 패턴 중지
            // TODO : 현재 임시로 반환
            // 원래라면 루틴 실행도 안되게 해야 함
            return;
        }

        if (!m_isBossStageInProgress)
        {
            StartBossStage();
        }
        else
        {
            // 죽었을 때도 루틴이 계속 발생
            if (currentEnemyState == EnemyState.DEAD)
            {

                if (bossResultUI != null)
                {
                    bossResultUI.OnBossDefated();
                    KillMushroom();
                }

                return;
            }

            LookAtPlayer();
            CheckPhaseTransition();
            AttackPattern();
        }
    }

    public override void TakeDamage(SkillType type, float damage, bool isTripleChain = false)
    {
        if (m_aliveMonsters.Count != 0 || m_isBossStageInProgress == false) return;
        base.TakeDamage(type, damage, isTripleChain);
    }

    private void StartBossStage()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance < startDistance)
        {
            monsterHealthUI.EnableHpUI();
            if (m_bossNameUI != null)
            {
                m_bossNameUI.gameObject.SetActive(true);
            }
            m_isBossStageInProgress = true;
            m_currentPhase = 1;
        }
    }

    private void AttackPattern()
    {
        if (isAttacking || isGroggy) return;

        m_attackCount++;
        Debug.Log($"공격 횟수 : {m_attackCount}");

        if (m_attackCount >= 5)
        {
            Debug.Log("공격 5회! 그로기 상태 진입");
            StartCoroutine(GroggyTime());
        }

        if (m_lastPattern == 0)
        {
            m_lastPattern = 1;
        }
        else
        {
            m_lastPattern = 0;
        }

        switch (m_lastPattern)
        {
            case 0:
                StartCoroutine(AttackPatternOne());
                break;
            case 1:
                StartCoroutine(AttackPatternTwo());
                break;
            default:
                break;
        }
    }

    private IEnumerator AttackPatternOne()
    {
        isAttacking = true;
        enemyAnimator.SetTrigger("MushBossAttack01");

        yield return new WaitForSeconds(1f);

        float damage = m_projectileDamage01;
        float speed = m_projectileSpeed01;

        if (m_currentPhase == 2)
        {
            damage *= 1.5f;
            speed *= 1.2f;
        }

        GameObject projectile = Instantiate(m_projectilePrefab01, m_firePosition01.position, Quaternion.identity);
        BossProjectile bossProjectile = projectile.GetComponent<BossProjectile>();
        bossProjectile.Init(BossProjectileType.Mush_1, player, damage, speed);

        yield return new WaitForSeconds(2f);
        isAttacking = false;
    }

    private IEnumerator AttackPatternTwo()
    {
        isAttacking = true;
        enemyAnimator.SetTrigger("MushBossAttack02");
        yield return new WaitForSeconds(1f);
        StartCoroutine(AttackPatternTwoCoroutine());
    }

    private IEnumerator AttackPatternTwoCoroutine()
    {
        for (int i = 0; i < m_firePositions02.Length; i++)
        {
            int random = Random.Range(i, m_firePositions02.Length);
            Transform temp = m_firePositions02[i];
            m_firePositions02[i] = m_firePositions02[random];
            m_firePositions02[random] = temp;
        }

        for (int i = 0; i < m_firePositions02.Length; i++)
        {
            float damage = m_projectileDamage02;
            float speed = m_projectileSpeed02;

            if (m_currentPhase == 2)
            {
                damage *= 1.5f;
                speed *= 1.2f;
            }

            GameObject projectile = Instantiate(m_projectilePrefab02, m_firePositions02[i].position, Quaternion.identity);
            BossProjectile bossProjectile = projectile.GetComponent<BossProjectile>();
            bossProjectile.Init(BossProjectileType.Mush_2, player, damage, speed, m_hitEffectPrefab02);

            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(2f);
        isAttacking = false;
    }

    private IEnumerator GroggyTime()
    {
        // TODO: 그로기 상태일 때 보스 행동 추가
        isGroggy = true;
        yield return new WaitForSeconds(m_groggyDuration);
        Debug.Log("그로기 상태 해제");
        m_attackCount = 0;
        isGroggy = false;
    }

    private void CheckPhaseTransition()
    {
        float currentHpRatio = healthPoint / monsterHealthUI.maxHealth;

        if (!hasSpawnedAt70 && currentHpRatio <= 0.7f)
        {
            hasSpawnedAt70 = true;
            StartCoroutine(SpawnMushroomWithTimer());
        }

        if (!hasSpawnedAt40 && currentHpRatio <= 0.4f)
        {
            hasSpawnedAt40 = true;
            StartCoroutine(SpawnMushroomWithTimer());
        }

        if (m_currentPhase == 1 && currentHpRatio <= 0.4f)
        {
            EnterPhaseTwo();
        }
    }

    private void EnterPhaseTwo()
    {
        // TODO: 2페이즈 진입 시 효과 추가
        Debug.Log("2페이즈 진입");
        m_currentPhase = 2;
    }

    private IEnumerator SpawnMushroomWithTimer()
    {
        SpawnMushrooms();
        yield return new WaitForSeconds(m_killTimeLimt);

        int aliveCount = 0;
        for (int i = m_aliveMonsters.Count - 1; i >= 0; i--)
        {
            if (m_aliveMonsters[i] != null)
            {
                aliveCount++;
                BaseMonster monster = m_aliveMonsters[i].GetComponent<BaseMonster>();
                if (monster != null)
                {
                    monster.ForceDie();
                }
            }
        }

        if (aliveCount > 0)
        {
            float recoverAmount = m_healPerMushroom * aliveCount;
            healthPoint += recoverAmount;

            if (healthPoint >= m_maxHealth)
            {
                healthPoint = m_maxHealth;
            }

            m_monsterHealthUI.UpdateHpUI();
        }
    }

    private void SpawnMushrooms()
    {
        // TODO: 몬스터가 생성될 때 생성되는 느낌 주기
        foreach (Transform spawnPoint in m_spawnPoints)
        {
            GameObject monster = Instantiate(m_mushroomPrefab, spawnPoint.position, Quaternion.identity);
            BaseMonster mushroom = monster.GetComponent<BaseMonster>();
            mushroom.SetSummonedByBoss(); // 보스가 소환한 몬스터로 설정
            Vector3 direction = (player.transform.position - monster.transform.position).normalized;
            monster.transform.rotation = Quaternion.LookRotation(direction);
            m_aliveMonsters.Add(monster);
        }
        Debug.Log("일반 버섯 몬스터 생성");
    }

    private void KillMushroom()
    {
        for (int i = m_aliveMonsters.Count - 1; i >= 0; i--)
        {
            if (m_aliveMonsters[i] != null)
            {
                m_aliveMonsters[i].GetComponent<BaseMonster>().ForceDie();
                m_aliveMonsters.RemoveAt(i);
            }
        }

        m_aliveMonsters.Clear();
    }

    public void RemoveMushroomList(GameObject mushroom)
    {
        m_aliveMonsters.Remove(mushroom);
    }

    private void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}

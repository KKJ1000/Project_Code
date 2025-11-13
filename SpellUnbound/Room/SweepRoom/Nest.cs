using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Nest : BaseMonster
{
    [Header("==== HealthPoint를 제외한 위 나머지 정보들은 무시 ====")]
    [Header("둥지 스폰 몬스터 세팅")]
    [SerializeField, Tooltip("플레이어 감지 시 최초 스폰 몬스터들")]
    private List<WaveMonsterInfo> m_initialMonster;
    [SerializeField, Tooltip("10초 간격으로 순차적으로 소환될 몬스터")]
    private List<WaveMonsterInfo> m_waveMonster;

    [SerializeField, Tooltip("인지 범위")]private float m_detectionRange = 5f;
    private bool hasSpawned = false;

    private float m_spawnInterval = 10f;
    private int m_nextSpawnIndex = 0;

    private void Update()
    {
        if (!hasSpawned)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if (distance <= m_detectionRange)
            { 
                hasSpawned = true;
                StartSpawn();
            }
        }
    }

    private void StartSpawn()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        foreach (var monsterInfo in m_initialMonster)
        {
            SpawnMonster(monsterInfo);
        }
        
        while (true) // 웨이브 몬스터 리스트 순회 end => start
        {
            if (m_waveMonster.Count == 0) yield break;

            yield return new WaitForSeconds(m_spawnInterval);

            SpawnMonster(m_waveMonster[m_nextSpawnIndex]);
            m_nextSpawnIndex++;

            if (m_nextSpawnIndex >= m_waveMonster.Count)
            {
                m_nextSpawnIndex = 0;
            }
        }
    }

    private void SpawnMonster(WaveMonsterInfo monsterInfo)
    {
        GameObject monster = Instantiate(monsterInfo.monsterPrefab, monsterInfo.spawnPoint.position, monsterInfo.spawnPoint.rotation);
        WaveManager.Instance.AddAliveMonster(monster);
    }

    public override void TakeDamage(SkillType type, float damage, bool isTripleChain = false)
    {
        // 만약에 거리 안에 안들어온 상태에서도 데미지 넣고 싶으면 이 함수 지우기
        if (hasSpawned)
        {
            base.TakeDamage(type, damage, isTripleChain);
        }
    }

    protected override void Die(SkillType type, bool isTripleChain)
    {
        if (_isDead) return;

        _isDead = true;
        Debug.Log("둥지가 파괴 되었습니다.");
        WaveManager.Instance.OnNestDestroyed(gameObject);
        StopAllCoroutines();
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_detectionRange);
    }
}

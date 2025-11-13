using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Test5;

public class WaveManager : MonoBehaviour
{
    [Header("웨이브 UI 관리")]
    [SerializeField] private WaveUI m_waveUI;
    [Header("플레이어 캐릭터")]
    [SerializeField] private GameObject m_player;
    [Header("클리어 보상 (보물 상자)")]
    [SerializeField] private GameObject m_treasureChestPrefab;
    private Vector3 m_lastKilledMonsterPosition;

    public static WaveManager Instance { get; private set; }

    private ERoomType roomType;

    private List<GameObject> m_aliveMonsters = new List<GameObject>();
    private List<GameObject> m_sweepNests = new List<GameObject>();
    private List<GameObject> m_droppedGolds = new List<GameObject>();

    private WaveData[] m_waveDatas;
    private Transform m_defenseCrytalTransform;
    private RoomGate m_roomGate;

    private int m_currentWaveIndex = 0;
    private float m_defenseRoomSpawnDelay;
    private bool isWaveInProgress = false;
    //private bool canMovePortalRoom = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (m_waveUI == null)
            m_waveUI = GameObject.Find("WaveUI")?.GetComponent<WaveUI>();
        if (m_player == null)
            m_player = GameObject.FindWithTag("Player");
    }

    public void SetupRoom(ERoomType roomType, WaveData[] newData, RoomGate roomGate, List<GameObject> sweepNests = null)
    {
        this.roomType = roomType;
        m_waveDatas = newData;
        m_roomGate = roomGate;
        m_currentWaveIndex = 0;

        if (roomType == ERoomType.Sweep && sweepNests != null)
        {
            m_sweepNests = sweepNests;
        }

        StartRoom();
    }

    private void StartRoom()
    {
        switch (roomType)
        {
            case ERoomType.Annihilation: // 섬멸
                StartAnnihilation();
                break;
            case ERoomType.Sweep:        // 소탕
                StartSweep();
                break;
            case ERoomType.Defense:      // 보호
                StartDefense();
                break;
        }
    }

    private void StartAnnihilation()
    {
        StartAnnihilationWave(0);
    }

    private void StartAnnihilationWave(int waveIndex)
    {
        if (isWaveInProgress) return;

        m_currentWaveIndex = waveIndex;
        isWaveInProgress = true;
        //m_waveUI.ActiveWaveText(m_currentWaveIndex);
        Invoke(nameof(SpawnAnnihilationWave), 1.0f);
    }

    private void SpawnAnnihilationWave()
    {
        WaveData waveData = m_waveDatas[m_currentWaveIndex];
        m_aliveMonsters.Clear();
        foreach (var monsterInfo in waveData.monsterInfos)
        {
            var monster = Instantiate(monsterInfo.monsterPrefab, monsterInfo.spawnPoint.position, monsterInfo.spawnPoint.rotation);
            Vector3 direction = (m_player.transform.position - monster.transform.position).normalized;
            monster.transform.rotation = Quaternion.LookRotation(direction);
            m_aliveMonsters.Add(monster);
        }
    }

    private void AnnihilationWaveClear()
    {
        isWaveInProgress = false;
        m_currentWaveIndex++;
        CollectAllRemainingGolds();

        if (m_currentWaveIndex < m_waveDatas.Length)
        {
            StartAnnihilationWave(m_currentWaveIndex);
        }
        else
        {
            TrySpawnTreasureChest();
            m_roomGate.OpenGate();
        }
    }

    private void StartSweep()
    {
        Debug.Log($"소탕방 진입. 둥지 {m_sweepNests.Count}개 존재");
    }

    public void OnNestDestroyed(GameObject nest)
    {
        if (m_sweepNests.Contains(nest))
        {
            m_sweepNests.Remove(nest);
            Debug.Log($"남은 둥지 수{m_sweepNests.Count}");

            if (m_sweepNests.Count == 0)
            {
                KillAliveMonsters();
                CollectAllRemainingGolds();
                TrySpawnTreasureChest(); // 확률 추가 보상 획득 & 방 이동

                m_roomGate.OpenGate();
            }
        }
    }

    private void StartDefense()
    {
        StartCoroutine(DefenseCoroutine());
    }

    private IEnumerator DefenseCoroutine()
    {
        float spawnInterval = 15f;
        float duration = 60f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return StartCoroutine(SpawnDefenseWaveCoroutine());
            yield return new WaitForSeconds(spawnInterval);
            elapsed += spawnInterval;
        }
    }

    private IEnumerator SpawnDefenseWaveCoroutine()
    {
        if (m_waveDatas.Length == 0 || m_waveDatas.Length > 1)
        {
            Debug.LogError("보호방 웨이브 데이터가 없거나 두 개 이상의 웨이브가 설정되었습니다.");
            yield break;
        }

        WaveData waveData = m_waveDatas[0];

        foreach (var monsterInfo in waveData.monsterInfos)
        {
            var monster = Instantiate(monsterInfo.monsterPrefab, monsterInfo.spawnPoint.position, monsterInfo.spawnPoint.rotation);
            BaseMonster baseMonster = monster.GetComponent<BaseMonster>();
            baseMonster.SetDefenseCrystal(m_defenseCrytalTransform);

            Vector3 direction = (m_player.transform.position - monster.transform.position).normalized;
            monster.transform.rotation = Quaternion.LookRotation(direction);
            m_aliveMonsters.Add(monster);

            yield return new WaitForSeconds(m_defenseRoomSpawnDelay);
        }
    }

    public void TrySpawnTreasureChest()
    {
        if (m_treasureChestPrefab == null)
        {
            Debug.LogWarning("보물 상자 프리팹이 설정되지 않았습니다.");
            return;
        }

        float chance = Random.Range(0, 10);
        if (chance < 5)
        {
            Instantiate(m_treasureChestPrefab, m_lastKilledMonsterPosition, Quaternion.identity);
            RoomManager.Instance.OnRoomCleared();

            //canMovePortalRoom = true;
            Debug.Log("보물상자(추가 보상) 있음, 획득하세요.");
        }
        else
        {


            Debug.Log("보물상자(추가 보상) 없음");
            // RoomManager.Instance.OnRoomCleared();
        }

        SpellRewardManager.Instance.GenerateAndShowRewards(() =>
        {
            PlayerInput.Instance.DisableCursor();
            PlayerUI.Instance.ShowTeleportUI();
        });
    }

    public void KillAliveMonsters()
    {
        for (int i = m_aliveMonsters.Count - 1; i >= 0; i--)
        {
            var monster = m_aliveMonsters[i];
            if (monster != null)
            {
                monster.GetComponent<BaseMonster>().ForceDie();
            }
        }
    }

    public void AddAliveMonster(GameObject monster)
    {
        m_aliveMonsters.Add(monster);
    }

    public void OnMonsterDead(GameObject monster)
    {
        if (m_aliveMonsters.Contains(monster))
        {
            m_aliveMonsters.Remove(monster);
        }

        m_lastKilledMonsterPosition = monster.transform.position;

        if (roomType == ERoomType.Annihilation)
        {
            if (m_aliveMonsters.Count == 0)
            {
                AnnihilationWaveClear();
            }
        }
    }

    public void RegisterGold(GameObject gold)
    {
        if (!m_droppedGolds.Contains(gold))
        {
            m_droppedGolds.Add(gold);
        }
    }

    public void UnregisterGold(GameObject gold)
    {
        if (m_droppedGolds.Contains(gold))
        {
            m_droppedGolds.Remove(gold);
        }
    }

    public void CollectAllRemainingGolds()
    {
        foreach (var gold in m_droppedGolds)
        {
            if (gold != null)
            {
                gold.GetComponent<ItemMagnet>().ForceCollect();
            }
        }
        m_droppedGolds.Clear();
    }

    public RoomGate RoomGateInfo()
    {
        return m_roomGate;
    }

    //public void SetCanMoveToPortalRoom(bool value)
    //{
    //    canMovePortalRoom = value;
    //}

    //public bool CanMoveToPortalRoom()
    //{
    //    return canMovePortalRoom;
    //}

    public void SetDefenseCrystal(Transform transform)
    {
        m_defenseCrytalTransform = transform;
    }

    public void SetRoomGate(RoomGate roomGate)
    {
        m_roomGate = roomGate;
    }

    public void SetDefenseSpawnDelay(float delay)
    {
        m_defenseRoomSpawnDelay = delay;
    }
}

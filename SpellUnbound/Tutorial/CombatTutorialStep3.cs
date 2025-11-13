using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTutorialStep3 : MonoBehaviour, ITutorialStep
{
    [SerializeField, Header("[다음방으로 가는 문]")] private TutorialTrigger m_tutorialExitTrigger; // 전투 튜토리얼 방3 클리어
    [SerializeField, Header("[가이드 UI]")] private GameObject m_popupUI;
    [SerializeField, Header("[몬스터 프리팹]")] private GameObject m_mushroomMonsterPrefab;
    [SerializeField, Header("[몬스터 소환 위치]")] private Transform[] m_spawnPoints;

    private bool isGuideUIActive = false;
    private bool hasSpawnedMonsters = false;
    private List<GameObject> m_spawnedMonsters = new List<GameObject>();

    public void Enter()
    {
        PlayerInput.Instance.SetPopupActive(true);
        m_popupUI.SetActive(true);
        isGuideUIActive = true;
        Time.timeScale = 0f;
    }

    void ITutorialStep.Update()
    {
        if (isGuideUIActive && Input.GetKeyDown(KeyCode.Escape))
        {
            isGuideUIActive = false;
            PlayerInput.Instance.SetPopupActive(false);
            m_popupUI.SetActive(false);
            Time.timeScale = 1f;
            SpawnMonsters();
        }

        if (hasSpawnedMonsters && !isGuideUIActive)
        {
            bool allDead = true;

            foreach (var monster in m_spawnedMonsters)
            {
                if (monster != null && !monster.GetComponent<BaseMonster>().IsDead)
                {
                    allDead = false;
                    break;
                }
            }

            if (allDead)
            {
                m_tutorialExitTrigger.gameObject.GetComponent<Collider>().isTrigger = true;
            }
        }
    }

    private void SpawnMonsters()
    {
        foreach (Transform point in m_spawnPoints)
        {
            var monster = Instantiate(m_mushroomMonsterPrefab, point.position, Quaternion.identity);
            monster.GetComponent<BaseMonster>().SetTutorialMonster();
            m_spawnedMonsters.Add(monster);
        }

        hasSpawnedMonsters = true;
    }

    public bool IsCompleted()
    {
        if (m_tutorialExitTrigger.IsTriggered)
        {
            return true;
        }
        return false;
    }

    public void Exit() { }
}

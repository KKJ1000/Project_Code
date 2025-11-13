using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTutorialStep2 : MonoBehaviour, ITutorialStep
{
    [SerializeField, Header("[다음방으로 가는 문]")] private TutorialTrigger m_tutorialExitTrigger; // 전투 튜토리얼 방2에서 나가는 Trigger
    [SerializeField, Header("[가이드 UI]")] private GameObject m_guideUI; // 1단계에서 활성화 시킨 가이드UI 비활성화 시킬꺼임...
    [SerializeField, Header("[몬스터 프리팹]")] private GameObject m_mushroomMonsterPrefab;
    [SerializeField, Header("[1번 몬스터 소환 위치]")] private Transform m_firstSpawnPoint;
    [SerializeField, Header("[2번 몬스터 소환 위치]")] private Transform[] m_secondSpawnPoints;

    private GameObject m_firstMonster;
    private List<GameObject> m_secondMonsters = new List<GameObject>();

    private bool isFirstSpawned = false;
    private bool isSecondSpawned = false;
    private bool m_guideUIDeactivated = false;

    public void Enter()
    {
        m_firstMonster = Instantiate(m_mushroomMonsterPrefab, m_firstSpawnPoint.position, Quaternion.identity);
        m_firstMonster.GetComponent<BaseMonster>().SetTutorialMonster();
        isFirstSpawned = true;
    }

    void ITutorialStep.Update()
    {
        if (isFirstSpawned && !isSecondSpawned && m_firstMonster == null)
        {
            foreach (var point in m_secondSpawnPoints)
            {
                var monster = Instantiate(m_mushroomMonsterPrefab, point.position, Quaternion.identity);
                monster.GetComponent<BaseMonster>().SetTutorialMonster();
                m_secondMonsters.Add(monster);
            }

            isSecondSpawned = true;
        }

        if (isSecondSpawned && !m_guideUIDeactivated)
        {
            bool allDead = true;

            foreach (var monster in m_secondMonsters)
            {
                if (monster != null && !monster.GetComponent<BaseMonster>().IsDead)
                {
                    allDead = false;
                    break;
                }
            }

            if (allDead)
            {
                m_guideUI.SetActive(false);
                m_guideUIDeactivated = true;
                m_tutorialExitTrigger.gameObject.GetComponent<Collider>().isTrigger = true;
            }
        }
    }

    public bool IsCompleted()
    {
        if (m_tutorialExitTrigger.IsTriggered) // 조작 튜토리얼에서 인벤토리 튜토리얼로 넘어가는 문과 충돌했다면
        {
            return true;
        }
        return false;
    }

    public void Exit() { }
}

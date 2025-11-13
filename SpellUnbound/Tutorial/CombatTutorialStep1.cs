using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTutorialStep1 : MonoBehaviour, ITutorialStep
{
    [SerializeField, Header("[다음방으로 가는 문]")] private TutorialTrigger m_tutorialExitTrigger; // 전투 튜토리얼 방1에서 나가는 Trigger
    [SerializeField, Header("[가이드 UI]")] private GameObject m_guideUI;
    [SerializeField, Header("[팝업 UI]")] private GameObject m_popupUI;
    [SerializeField, Header("[몬스터 프리팹]")] private GameObject m_mushroomMonsterPrefab;
    [SerializeField, Header("[몬스터 소환 위치]")] private Transform m_spawnPoint;

    private GameObject m_monster;
    private bool isPopupActive = false;
    private bool hasMonsterSpawned = false;

    public void Enter()
    {
        m_guideUI.SetActive(true); // 공격 방식을 설명해주는 UI
        SpawnTutorialMonster();
    }

    void ITutorialStep.Update()
    {
        if (m_monster != null && !isPopupActive && hasMonsterSpawned && m_monster.GetComponent<BaseMonster>().IsDead)
        {
            isPopupActive = true;
            Invoke(nameof(ShowPopupUI), 1.5f);
        }

        if (m_popupUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePopupUI();
        }
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

    private void SpawnTutorialMonster()
    {
        m_monster = Instantiate(m_mushroomMonsterPrefab, m_spawnPoint.position, Quaternion.identity);
        m_monster.GetComponent<BaseMonster>().SetTutorialMonster();
        hasMonsterSpawned = true;
    }

    private void ShowPopupUI()
    {
        PlayerInput.Instance.SetPopupActive(true);
        m_popupUI.SetActive(true);
        Time.timeScale = 0f;
    }

    private void ClosePopupUI()
    {
        Time.timeScale = 1f;
        PlayerInput.Instance.SetPopupActive(false);
        m_popupUI.SetActive(false);
        m_tutorialExitTrigger.gameObject.GetComponent<Collider>().isTrigger = true;
    }
}

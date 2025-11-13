using UnityEngine;

/// <summary>
/// 인벤토리 튜토리얼 단계
/// </summary>
public class InventoryTutorialStep : MonoBehaviour, ITutorialStep
{
    [SerializeField, Header("[다음방으로 가는 문]")] private TutorialTrigger m_tutorialExitTrigger; // 인벤토리 튜토리얼 방에서 나가는 Trigger
    [SerializeField, Header("[인벤토리 팝업 UI]")] private GameObject m_popupObject;
    [SerializeField, Header("[상호작용 안내 UI]")] private GameObject m_interactKeyPrompt;
    [SerializeField, Header("[Tab키 클릭 안내 UI]")] private GameObject m_tabKeyPrompt;
    [SerializeField, Header("[스킬 아이템]")] private GameObject m_skillItemObject;
    [SerializeField, Header("[획득할 스킬 SO]")] private PlayerSkillSO m_skillSO;

    private GameObject m_player;
    private bool hasExecuted = false;
    private bool isPopupActive = false;
    private bool hasItem = false;

    public void Enter()
    {
        m_player = GameObject.FindWithTag("Player");
    }

    void ITutorialStep.Update()
    {
        if (isPopupActive && Input.GetKeyDown(KeyCode.Escape))
        {
            m_popupObject.SetActive(false);
            PlayerInput.Instance.SetPopupActive(false);
            m_tutorialExitTrigger.gameObject.GetComponent<Collider>().isTrigger = true;
            Time.timeScale = 1f;
        }

        if (m_tabKeyPrompt.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                m_tabKeyPrompt.SetActive(false);
                m_popupObject.SetActive(true);
                PlayerInput.Instance.SetPopupActive(true);
                isPopupActive = true;
                Time.timeScale = 0f;
            }
        }

        if (m_skillItemObject == null) return;

        if (Vector3.Distance(m_player.transform.position, m_skillItemObject.transform.position) <= 5f && !hasExecuted)
        {
            m_interactKeyPrompt.SetActive(true);

            if (!hasItem && Input.GetKeyDown(KeyCode.E))
            {
                hasExecuted = true;
                hasItem = true;
                m_interactKeyPrompt.SetActive(false);
                m_tabKeyPrompt.SetActive(true);
                Destroy(m_skillItemObject);
                SkillInventory skillInventory = m_player.GetComponent<SkillInventory>();

                if (skillInventory != null)
                {
                    skillInventory.AddSkill(m_skillSO);

                    if (SkillInventoryUIManager.Instance != null)
                    {
                        SkillInventoryUIManager.Instance.GenerateSkillButtons();
                    }
                }
            }
        }
        else
        {
            m_interactKeyPrompt.SetActive(false);
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
}

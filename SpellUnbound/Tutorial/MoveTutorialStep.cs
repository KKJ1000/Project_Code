using UnityEngine;
using DG.Tweening;

/// <summary>
/// 조작 튜토리얼 단계
/// </summary>
public class MoveTutorialStep : MonoBehaviour, ITutorialStep
{
    [SerializeField] private TutorialTrigger m_tutorialExitTrigger;
    [SerializeField] private GameObject m_popupObject;

    private void Awake()
    {
        PlayerInput.Instance.DisableCursor();
    }

    public void Enter()
    {
        if (m_popupObject != null)
        {
            Invoke("EnableFirstPopup", 1);
        }
        else
        {
            Debug.LogError("팝업 오브젝트가 연결되지 않았습니다.");
        }
    }

    public void EnableFirstPopup()
    {
        m_popupObject.SetActive(true);
    }

    public bool IsCompleted()
    {
        if (m_tutorialExitTrigger.IsTriggered) // 조작 튜토리얼에서 인벤토리 튜토리얼로 넘어가는 문과 충돌했다면
        {
            return true;
        }
        return false;
    }

    public void Exit()
    {
        m_popupObject.SetActive(false);
    }

    void ITutorialStep.Update() { }
}
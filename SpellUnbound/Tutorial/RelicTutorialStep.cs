using System.Collections;
using UnityEngine;

public class RelicTutorialStep : MonoBehaviour, ITutorialStep
{
    [SerializeField, Header("[튜토리얼 종료 트리거]")] private TutorialTrigger m_tutorialExitTrigger;
    [SerializeField, Header("[유물 아이템 오브젝트]")] private GameObject m_relicObject;
    [SerializeField, Header("[인터랙션 UI]")] private GameObject m_interactKeyPrompt;
    [SerializeField, Header("[유물 팝업 UI]")] private GameObject m_popupObject;
    [SerializeField, Header("[왼쪽 문 오브젝트]")] private GameObject m_leftDoorObject;
    [SerializeField, Header("[오른쪽 문 오브젝트]")] private GameObject m_rightDoorObject;

    private GameObject m_player;
    private RelicNPC m_relicNPC;
    private bool isPopupActive = false;

    public void Enter() 
    {
        m_player = GameObject.FindWithTag("Player");
        m_popupObject.SetActive(false);
        m_relicNPC = m_relicObject.GetComponent<RelicNPC>();
    }

    void ITutorialStep.Update()
    {
        if (m_player != null)
        {
            if (!isPopupActive && Vector3.Distance(m_relicObject.transform.position, m_player.transform.position) <= 5f && m_relicNPC.CanInteract(m_player))
            {
                m_interactKeyPrompt.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E) && !isPopupActive)
                {
                    isPopupActive = true;
                    m_relicNPC.OnInteract(m_player);
                    Destroy(m_relicObject);
                    m_interactKeyPrompt.SetActive(false);
                    m_popupObject.SetActive(true);
                    PlayerInput.Instance.SetPopupActive(true);
                }
            }
            else
            {
                m_interactKeyPrompt.SetActive(false);
            }

            if (isPopupActive)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    m_tutorialExitTrigger.gameObject.GetComponent<Collider>().isTrigger = true;
                    m_popupObject.SetActive(false);
                    PlayerInput.Instance.SetPopupActive(false);
                }
            }
        }
    }

    public void Exit()
    {
        StartCoroutine(RotateDoor(m_leftDoorObject.transform, 0f, 95f, 1.5f));
        StartCoroutine(RotateDoor(m_rightDoorObject.transform, -180f, -275f, 1.5f));
    }

    private IEnumerator RotateDoor(Transform doorTransform, float fromY, float toY, float duration)
    {
        float elapsed = 0f;
        Quaternion startRotation = Quaternion.Euler(0f, fromY, 0f);
        Quaternion endRotation = Quaternion.Euler(0f, toY, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / duration);
            doorTransform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
        }

        doorTransform.rotation = endRotation;
    }


    public bool IsCompleted()
    {
        if (m_tutorialExitTrigger.IsTriggered) // 조작 튜토리얼에서 인벤토리 튜토리얼로 넘어가는 문과 충돌했다면
        {
            return true;
        }
        return false;
    }
}

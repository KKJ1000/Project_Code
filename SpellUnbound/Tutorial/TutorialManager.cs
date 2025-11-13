using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> m_stepObjects; // ITutorialStep이 상속된 스크립트가 붙은 오브젝트만 연결.

    public static TutorialManager Instance { get; private set; }
    public int CurrentStepIndex { get; private set; } = 0;

    private List<ITutorialStep> m_steps = new List<ITutorialStep>();
    private bool isTutorialStarted = false;
    private bool isTutorialEnded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (var obj in m_stepObjects)
        {
            ITutorialStep step = obj.GetComponent<ITutorialStep>();

            if (step != null)
            {
                m_steps.Add(step);
            }
            else
            {
                Debug.LogError($"{obj.name}에 ITutorialStep을 상속받은 컴포넌트가 존재하지 않습니다.");
            }
        }

        StartTutorial();
    }

    public void StartTutorial()
    {
        if (m_steps.Count > 0)
        {
            PlayerInput.Instance.LockAllInputs(false);
            isTutorialStarted = true;
            m_steps[0].Enter(); // 0번 인덱스 조작 방법 튜토리얼 연결 후 시작 시 조작 방법 팝업이 뜨며 플레이어는 해당 팝업을 보고 플레이어를 이동 가능.
        }
    }

    private void Update()
    {
        if (!isTutorialStarted || isTutorialEnded) return;

        ITutorialStep currentStep = m_steps[CurrentStepIndex];
        currentStep.Update();

        if (currentStep.IsCompleted())
        {
            currentStep.Exit();
            CurrentStepIndex++;

            if (CurrentStepIndex >= m_steps.Count)
            {
                isTutorialEnded = true;
            }
            else
            {
                m_steps[CurrentStepIndex].Enter();
            }
        }
    }
}

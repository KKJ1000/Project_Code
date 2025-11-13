using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerController : MonoBehaviour
{
    public Transform[] waypoints; // 손님이 이동할 웨이포인트
    public float speed = 2f; // 손님 이동속도
    public bool isSuccess = false; // 포션 구매 체크

    private int currentWaypointIndex = 0; // 현재 웨이 포인트 인덱스

    [Header("손님이 원하는 포션 UI")]
    public Image potionUI;
    public Image potionBoxUI;

    [Header("타이머 설정")]
    public TimerUI timerController; // TimerController 오브젝트 (타이머를 컨트롤하는 오브젝트)
    public GameObject timerObject; // 비활성화 상태로 시작하는 타이머 오브젝트

    private bool isWaiting = false; // 타이머가 실행 중인지 확인

    void Update()
    {
        MoveToWaypoint();
    }

    void MoveToWaypoint()
    {
        if (waypoints == null)
        {
            Debug.LogError("손님이 갈 웨이포인트가 지정되지 않았습니다.");
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = targetWaypoint.position - transform.position;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);
        transform.position = newPosition;

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.5f)
        {
            if (currentWaypointIndex == 0)
            {
                potionUI.gameObject.SetActive(true);
                potionBoxUI.gameObject.SetActive(true);

                if (!isWaiting)
                {
                    isWaiting = true;
                    timerObject.SetActive(true);
                    timerController.StartTimer();
                    StartCoroutine(WaitAndLeave());
                }

                if (isSuccess)
                {
                    Vector3 scale = transform.localScale;
                    scale.x = -1;
                    transform.localScale = scale;

                    currentWaypointIndex++;
                    potionUI.gameObject.SetActive(false);
                    potionBoxUI.gameObject.SetActive(false);
                    timerObject.SetActive(false);

                    GameManager.Instance.ChangeRating(1); // 구매 성공 시 평점 증가
                    StopCoroutine(WaitAndLeave());
                }
            }
            else if (currentWaypointIndex == 1)
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator WaitAndLeave()
    {
        yield return new WaitForSeconds(timerController.timerDuration);

        if (!isSuccess)
        {
            Vector3 scale = transform.localScale;
            scale.x = -1;
            transform.localScale = scale;

            currentWaypointIndex++;
            potionUI.gameObject.SetActive(false);
            potionBoxUI.gameObject.SetActive(false);
            timerObject.SetActive(false);

            GameManager.Instance.ChangeRating(-1); // 구매 실패 시 평점 감소
        }
    }
}

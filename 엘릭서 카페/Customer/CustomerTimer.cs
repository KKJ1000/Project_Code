using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerTimer : MonoBehaviour
{
    [Header("타이머 설정")]
    public float timerDuration = 50f; // 타이머 지속 시간
    public Image timerImage;          // 타이머 UI 이미지 (FillImage 사용)

    private bool isTimerRunning = false;
    private CustomerCtrl customerCtrl;


    void Start()
    {
        timerImage.enabled = true;
        customerCtrl = FindObjectOfType<CustomerCtrl>();
    }

    public void StartTimer()
    {
        if (!isTimerRunning) //타이머가 시작하지 않았다면
        {
            isTimerRunning = true; //시작 상태로 변경
            StartCoroutine(RunTimer());
        }    
    }

    IEnumerator RunTimer()
    {
        float elapsedTime = 0f;

        while (elapsedTime < timerDuration)
        {
            elapsedTime += Time.deltaTime;

            //FillAmount를 반시계 방향으로 채우기
            timerImage.fillAmount = 1 - (elapsedTime / timerDuration);
            yield return null;
        }

        // 타이머 종료 시 손님 이동
        TimerEndMove();
    }

    private void TimerEndMove()
    {
        //타이머가 끝나면 메서드 호출
        customerCtrl.OnCustomerTimerEnd(gameObject);
    }

    public void SellPotion()
    {
        // 포션 판매시 타이머 종료 및 손님 정지
        timerImage.enabled = false;
        StopAllCoroutines();
        isTimerRunning = false;
    }
}

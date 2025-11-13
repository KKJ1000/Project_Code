using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerCtrl : MonoBehaviour
{
    [Header("손님 관련 설정")]
    public GameObject[] customerPrefabs; // 손님 프리팹 리스트 (이중에서 랜덤으로 한명씩 생성)
    public Transform spawnPotion;        // 손님 생성 위치
    public Transform counterFront;       // 카운터 앞(손님 구매 위치)
    public Transform counterSide;        // 카운터 옆(손님 대기 위치)
    public Transform exitPosition;       // 손님이 사라질 위치

    private Queue<GameObject> activeCustomers = new Queue<GameObject>(); // 활성화된 손님 리스트

    public PlayerFame playerFame; //플레이어 명성 관리 

    //카운터 옆에서 카운터 앞까지 이동하는 시간
    //카운터에서 사라지는 위치까지 가는 시간은 1.0f면 적당함

    void Start()
    {
        playerFame = FindObjectOfType<PlayerFame>();
        StartCoroutine(CustomerSpawnRoutine()); // 손님 생성 코루틴 시작
    }

    IEnumerator CustomerSpawnRoutine()
    {
        while (true)
        {
            if (activeCustomers.Count < 2) // 손님의 수가 0명이거나 1명일때
            {
                SpawnCustomer();
            }
            yield return new WaitForSeconds(7f); // 생성 대기 시간
        }
    }

    // 손님 생성
    private void SpawnCustomer()
    {
        // 손님 프리팹 랜덤 선택
        GameObject customerPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Length)];
        GameObject customer = Instantiate(customerPrefab, spawnPotion.position, Quaternion.identity);
        activeCustomers.Enqueue(customer); // 생성된 손님 오브젝트 큐에 넣기

        // 첫번째 손님 카운터 앞, 두번째 손님은 카운터 옆으로 이동
        if (activeCustomers.Count == 1)
        {
            MoveCustomer(customer, counterFront.position, 3.5f); // 첫 손님은 카운터 앞
        }
        else if (activeCustomers.Count == 2)
        {
            MoveCustomer(customer, counterSide.position, 2.5f); // 두 번째 손님은 카운터 옆
        }

        //타이머 시작
        StartCoroutine(StartTimeWithDelay(customer));

    }

    // N초 후 타이머 시작
    private IEnumerator StartTimeWithDelay(GameObject customer)
    {
        yield return new WaitForSeconds(1.5f); // 손님이 생성위치에서 카운터까지 이동하는 시간=> ()에 넣기
        customer.GetComponent<CustomerTimer>().StartTimer();
    }

    // 손님 이동
    private void MoveCustomer(GameObject customer, Vector3 targetPosition, float duration)
    {
        StartCoroutine(MoveToPosition(customer.transform, targetPosition, duration));
    }

    // 손님 위치 이동 코루틴
    IEnumerator MoveToPosition(Transform customer, Vector3 targetPosition, float duration)
    {
        if (customer == null) yield break; // 오브젝트가 null이면 코루틴 종료

        Vector3 startPosition = customer.position; // 시작 위치는 손님의 현재 위치
        float elapsedTime = 0f; // 경과 시간

        while (elapsedTime < duration)
        {
            if (customer == null) yield break; // 오브젝트가 null이면 코루틴 종료

            elapsedTime += Time.deltaTime;
            customer.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            yield return null;
        }

        if (customer != null)
        {
            customer.position = targetPosition; // 목표 위치에 정확히 도달
        }
    }

    // 손님 판매 완료 처리
    public void SellPotion()
    {
        if (activeCustomers.Count > 0)
        {
            // 카운터 앞 손님 처리
            GameObject frontCustomer = activeCustomers.Dequeue();
            MoveCustomer(frontCustomer, exitPosition.position, 1.0f); // 손님을 퇴장 위치로 이동

            // 카운터 옆 손님을 앞으로 이동
            if (activeCustomers.Count > 0)
            {
                GameObject sideCustomer = activeCustomers.Peek(); // 카운터 옆 손님
                MoveCustomer(sideCustomer, counterFront.position, 2.0f); // 카운터 앞으로 이동
            }

            // 손님 제거 처리
            StartCoroutine(RemoveCustomerAfterDelay(frontCustomer, 1.0f)); // 사라질 위치로 도착 후 제거
        }
    }

    // 타이머가 끝난 손님을 처리하는 메서드
    public void OnCustomerTimerEnd(GameObject timeOutCustomer)
    {
        // 타이머가 끝난 손님한테는 포션 판매를 못했기 때문에 - 명성
        playerFame.DecreaseFame(0.5f);
        // 타이머가 끝난 손님을 퇴장 위치로 이동
        MoveCustomer(timeOutCustomer, exitPosition.position, 1.0f);

        // 손님이 이동 후, 큐에서 제거
        activeCustomers.Dequeue();

        // 큐에서 다음 손님이 있으면 카운터 앞으로 이동
        if (activeCustomers.Count > 0)
        {
            GameObject nextCustomer = activeCustomers.Peek();
            MoveCustomer(nextCustomer, counterFront.position, 2.0f);
        }

        // 손님 제거
        StartCoroutine(RemoveCustomerAfterDelay(timeOutCustomer, 1.0f));
    }

    // 손님 제거 처리
    IEnumerator RemoveCustomerAfterDelay(GameObject customer, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (customer != null)
        {
            Destroy(customer);
        }
    }
}

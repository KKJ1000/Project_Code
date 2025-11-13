using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerManager : MonoBehaviour
{
    public GameObject[] customerPrefabs; // 손님 프리팹 배열
    public Transform spawnPoint;         // 손님 생성 위치
    public Transform counterFront;       // 카운터 앞 위치
    public Transform counterSide;        // 카운터 옆 (손님 대기 공간)
    public Transform exitPoint;          // 손님이 포션 구매 이후 이동할 위치

    private float spawnDelay = 3f;       // 손님 생성 딜레이
    private float moveSpeed = 4f;        // 손님 이동 속도

    private Queue<GameObject> waitingCustomers = new Queue<GameObject>();  // 대기 중인 손님
    private bool isFrontOccupied = false;  // 카운터 앞 손님 여부 (카운터에 손님이 있다면 다음 손님이 카운터 옆으로 이동하도록)

    void Start()
    {
        StartCoroutine(SpawnCustomer());
    }

    IEnumerator SpawnCustomer()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnDelay);

            // 손님 생성 조건 확인
            if (waitingCustomers.Count < 2) // 카운터 옆과 앞을 합쳐 2명까지만 생성
            {
                GameObject newCustomer = Instantiate(customerPrefabs[Random.Range(0, customerPrefabs.Length)], spawnPoint.position, Quaternion.identity);
                waitingCustomers.Enqueue(newCustomer); // 생성된 손님 큐에 추가
                MoveCustomerToPosition(newCustomer); // 손님 생성 직후 이동 처리
            }
        }
    }

    private void MoveCustomerToPosition(GameObject customer)
    {
        if (!isFrontOccupied) // 카운터 앞이 비어 있는 경우
        {
            isFrontOccupied = true; // 손님이 이동하기 전 점유 상태로 변경
            StartCoroutine(MoveToPosition(customer, counterFront.position, () =>
            {
                // 첫 번째 손님이 떠나면 두 번째 손님이 카운터 앞으로 올 수 있게 함
                isFrontOccupied = true;
            })); // 카운터 앞 손님 있음으로 변경
        }
        else if (waitingCustomers.Count > 1) // 카운터 옆으로 대기
        {
            StartCoroutine(MoveToPosition(customer, counterSide.position, null));
        }
    }

    IEnumerator MoveToPosition(GameObject customer, Vector3 targetPosition, System.Action onComplete)
    {
        while (customer != null && Vector3.Distance(customer.transform.position, targetPosition) > 0.1f)
        {
            customer.transform.position = Vector3.MoveTowards(customer.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (customer != null)
        {
            customer.transform.position = targetPosition;
            onComplete?.Invoke(); // 콜백 호출
        }
    }

    public void HandleCustomerExit(GameObject customer)
    {
        StartCoroutine(MoveToPosition(customer, exitPoint.position, () =>
        {
            Destroy(customer, 2f); // 사라질 위치로 이동 후 2초 뒤 파괴

            if (customer == waitingCustomers.Peek()) // 현재 큐의 첫 번째 손님이 떠났다면
            {
                isFrontOccupied = false; // 카운터 앞이 비어 있음
                waitingCustomers.Dequeue(); // 떠난 손님을 큐에서 제거
                // 첫 번째 손님이 떠나면 두 번째 손님을 카운터 앞에 보내도록 함
                if (waitingCustomers.Count > 0)
                {
                    GameObject nextCustomer = waitingCustomers.Peek();
                    MoveCustomerToPosition(nextCustomer); // 두 번째 손님을 카운터 앞에 보내기
                }

                // 새로운 손님을 생성하고 카운터 옆으로 보내기
                if (waitingCustomers.Count < 2)
                {
                    GameObject newCustomer = Instantiate(customerPrefabs[Random.Range(0, customerPrefabs.Length)], spawnPoint.position, Quaternion.identity);
                    waitingCustomers.Enqueue(newCustomer); // 새 손님을 큐에 추가
                    MoveCustomerToPosition(newCustomer); // 카운터 옆으로 이동
                }
            }
        }));
    }
}

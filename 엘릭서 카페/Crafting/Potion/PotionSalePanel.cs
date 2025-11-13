using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class PotionSalePanel : MonoBehaviour
{
    public GameObject potionSalePanel;             //판매 창 오브젝트
    public PotionStorageSlot[] potionStorageSlots; //포션 보관 슬롯
    public SaleSlot[] saleSlots;                   //판매 슬롯 
    public Image customerImage;                    //손님 이미지 UI
    public Image potionImage;                      //손님이 원하는 포션 이미지 UI

    private bool isPanelActive = false;    // 패널 활성화 여부
    private bool isPlayerInRange = false;  // 플레이어 충돌 여부
    private Customer currentCustomer;      // 현재 손님 객체
    private PlayerMoney playerMoney;       // 플레이어 돈 관리 클래스


    void Start()
    {
        playerMoney = FindObjectOfType<PlayerMoney>(); //씬에서 PlayerMoney찾기
        TogglePanel(false); //시작 시 포션 판매 패널 비활성화
        UpdateSaleSlot();   //시작 시 포션 판매 패널 판매 슬롯 비활성화
    }

    void Update()
    {
        // Player가 충돌 범위 내에 있을 때 키보드 E키를 눌러 포션 판매 패널을 활성화/비활성화
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TogglePanel(!isPanelActive);
        }
    }

    // 포션 판매 패널 활성화/비활성화
    public void TogglePanel(bool isActive)
    {
        isPanelActive = isActive;
        
        if (isPanelActive) // 포션 판매 패널을 활성화
        {
            potionSalePanel.transform.localScale = Vector3.one; // 패널의 스케일 크기를 1,1,1로 설정하여 보이게 설정
            UpdateSaleSlot();     // 판매 슬롯 업데이트
            UpdateCustomerInfo(); // 손님 정보 업데이트
        }
        else // 포션 판매 패널을 비활성화
        {
            potionSalePanel.transform.localScale = Vector3.zero; // 패널의 스케일 크기를 0,0,0으로 설정하여 안보이게 설정
        }
    }

    // 판매 슬롯 상태 업데이트
    public void UpdateSaleSlot()
    {
        for (int i = 0; i < saleSlots.Length; i++)
        {
            if (i < potionStorageSlots.Length) // 보관 슬롯 개수와 일치
            {
                var storageSlot = potionStorageSlots[i];
                var saleSlot = saleSlots[i];

                if (storageSlot.currentPotion != null) //보관 슬롯에 포션이 존재하는 경우
                {
                    //포션 보관 {i+1}번째 슬롯과 포션 판매 슬롯 {i+1}번째 슬롯이 동기화 되었습니다.
                    saleSlot.SetPotion(storageSlot.currentPotion); //판매 슬롯에 포션 표시
                    saleSlot.Initialize(currentCustomer, playerMoney); //슬롯에 손님과 플레이어 정보 전달
                }
                else //보관 슬롯이 비어있는 경우
                { 
                    saleSlot.ClearPotion(); // 판매 슬롯도 비우기
                }
            }
        }
    }

    // 손님 정보 업데이트 (손님의 이미지와 손님이 원하는 포션 이미지 띄우기)
    public void UpdateCustomerInfo()
    {
        if (currentCustomer != null)
        {
            customerImage.sprite = currentCustomer.customerSprite;
            customerImage.enabled = true;
            potionImage.sprite = currentCustomer.desiredPotion.GetComponent<Image>().sprite;
            potionImage.enabled = true;
            customerImage.rectTransform.localScale = new Vector3(-1, 1, 1); //좌우 반전 (오른쪽으로 보고 있는 손님 캐릭터 이미지를 왼쪽으로 반전)
        }
        else
        {
            customerImage.sprite = null;
            customerImage.enabled = false;
            potionImage.sprite = null;
            customerImage.enabled = false;
        }
    }

    // 충돌 시작 체크
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }

        // 손님이 카운터에 들어왔을 때
        if (other.CompareTag("Customer"))
        {
            currentCustomer = other.GetComponent<Customer>();  // 손님 객체를 currentCustomer에 할당
            if (currentCustomer != null)
            {
                UpdateCustomerInfo();  // 손님이 들어왔을 때 정보 갱신
                Debug.Log("손님이 충돌 했습니다.");
            }
        }
    }

    // 충돌 종료 체크
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (isPanelActive)
            {
                TogglePanel(false); // 범위 나가면 패널 끄기
            }
        }

        // 손님이 카운터에서 나갔을 때
        if (other.CompareTag("Customer"))
        {
            currentCustomer = null;  // 손님 객체 제거
            UpdateCustomerInfo();
        }
    }
}

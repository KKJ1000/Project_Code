using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 판매 슬롯 클래스
public class SaleSlot : MonoBehaviour
{
    public PotionSalePanel potionSalePanel;
    public GameObject currentPotion;  // 슬롯에 표시되는 포션
    public Image potionImage;         // 포션 UI 이미지 (판매 슬롯에서 보여줄 이미지)
    [SerializeField] private Button sellButton; // 판매 버튼

    private Customer currentCustomer; // 현재 손님 정보
    private PlayerMoney playerMoney;  // 플레이어 돈 관리 클래스
    private PlayerFame playerFame;    // 플레이어 평판 관리 클래스
    private int potionPrice = 100; // 포션 가격 (임시)
    //private CustomerManager customerManager;
    private CustomerCtrl customerCtrl;

    [Header("튜토리얼 모드")]
    [SerializeField] private bool tutorialMode = false; // 튜토리얼 모드 여부 (인스펙터에서 설정 가능)

    [Header("손님이 원하는 포션을 판매 시 효과음")]
    public AudioClip sellPotionSFX;

    [Header("손님이 원하지 않는 포션을 판매 시 효과음")]
    public AudioClip failSellPotionSFX;

    private AudioSource audioSource; //오디오 소스 컴포넌트

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        sellButton.onClick.AddListener(SellPotion);
        ClearPotion(); // 시작 시 슬롯 비우기
        //customerManager = FindObjectOfType<CustomerManager>();
        customerCtrl = FindObjectOfType<CustomerCtrl>();
        potionSalePanel = FindObjectOfType<PotionSalePanel>();
        playerFame = FindObjectOfType<PlayerFame>();
    }

    public void Initialize(Customer customer, PlayerMoney money)
    {
        currentCustomer = customer;
        playerMoney = money;

        if (playerMoney == null)
        {
            Debug.LogError("Initialize 실패: playerMoney가 null입니다. 스크립트 연결 확인 필요.");
        }
    }

    // 판매 슬롯에 포션 설정
    public void SetPotion(GameObject potion)
    {
        currentPotion = potion;

        if (potionImage != null)
        {
            // 포션 오브젝트의 Image 컴포넌트 가져오기
            var potionImageComponent = currentPotion.GetComponent<Image>();
            if (potionImageComponent != null)
            {
                potionImage.sprite = potionImageComponent.sprite;
                potionImage.enabled = true; // 포션 이미지 활성화
            }
            else
            {
                Debug.LogWarning("포션 오브젝트에 Image 컴포넌트가 없습니다.");
            }
        }
    }

    // 판매 슬롯 비우기
    public void ClearPotion()
    {
        currentPotion = null;

        if (potionImage != null)
        {
            potionImage.sprite = null; // 이미지 초기화
            potionImage.enabled = false; // 이미지 비활성화
        }
    }

    // 포션 판매 처리
    public void SellPotion()
    {
        var currentPotionScript = currentPotion.GetComponent<Potion>();
        var desiredPotionScript = currentCustomer.desiredPotion.GetComponent<Potion>();

        // 손님이 원하는 포션과 슬롯의 포션 비교
        if (currentPotionScript.potionID == desiredPotionScript.potionID)
        {
            audioSource.clip = sellPotionSFX;
            audioSource.Play();
            playerMoney.AddMoney(potionPrice); // 플레이어에게 돈 추가
            playerFame.IncreaseFame(0.5f); //평판 증가
            if (tutorialMode)
            {
                // 튜토리얼 모드에서는 손님 객체를 즉시 삭제
                Destroy(currentCustomer.gameObject);
            }
            else
            {
                // 일반 모드에서는 기존 손님 이동 로직 사용
                customerCtrl.SellPotion(); //손님이 포션 구매 이후 이동
                //customerManager.HandleCustomerExit(currentCustomer.gameObject);
            }

            ClearPotionAfterSale();
        }
        else
        {
            audioSource.clip = failSellPotionSFX;
            audioSource.Play();
            if (tutorialMode)
            {
                Destroy(currentCustomer.gameObject);
            }
            else
            {
                customerCtrl.SellPotion(); //손님이 포션 구매 이후 이동
                playerFame.DecreaseFame(0.5f); // 평판 감소
                //customerManager.HandleCustomerExit(currentCustomer.gameObject);
            }

            ClearPotionAfterSale();
        }
    }

    // 판매 후 슬롯 및 상태 초기화
    private void ClearPotionAfterSale()
    {
        currentCustomer = null;

        if (currentPotion != null)
        {
            Destroy(currentPotion.gameObject); // 포션 오브젝트 파괴
            ClearPotion(); // 판매 슬롯 클리어
        }

        if (potionSalePanel != null)
        {
            potionSalePanel.TogglePanel(false);
        }
        else
        {
            Debug.LogError("potionSalePanel이 null입니다. 패널 객체 연결을 확인하세요.");
        }
    }
}
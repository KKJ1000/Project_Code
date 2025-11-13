using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//프린터 가공 스크립트
public class PrinterProcessing : MonoBehaviour
{
    [SerializeField] private GameObject panel; //프린터 가공 UI 패널 오브젝트
    public Ingredient[] ingredients;           //가공 할 수 있는 재료 목록 (알파스크롤, 베타스크롤)
    public Slider[] progressBars;              //가공 진행도 표시 슬라이더
    public Image[] fillImages;                 //슬라이더 Fill 이미지

    private bool isPanelActive;                //패널 활성화 여부
    private bool isPlayerInRange;              //플레이어 충돌 여부

    private bool[] isProcessingFlags;          //가공중인지 여부

    private PlayerMoney playerMoney;


    void Start()
    {
        isProcessingFlags = new bool[ingredients.Length];

        //슬라이더 Fill 이미지 비활성화
        foreach (var fill in fillImages)
        {
            fill.enabled = false;
        }
    }


    void Update()
    {
        //플레이어가 충돌 범위 안에 있을 때 E키를 누르면 패널 토글
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TogglePanel();
        }

        //가공 패널이 활성화 되어있을 때에만 키 입력 가능
        if (isPanelActive)
        {
            if (Input.GetKeyDown(KeyCode.B) && !isProcessingFlags[0])
            {
                StartProcessing(ingredients[0], 3.0f, 0);
            }
            else if (Input.GetKeyDown(KeyCode.N) && !isProcessingFlags[1])
            {
                StartProcessing(ingredients[1], 3.0f, 1);
            }
        }
    }

    private void TogglePanel()
    {
        isPanelActive = !isPanelActive;
        panel.SetActive(isPanelActive);
    }

    //재료 가공 시작
    private void StartProcessing(Ingredient ingredient, float processingTime, int index)
    {
        isProcessingFlags[index] = true;
        //가공중 표시할 슬라이더 Fill 활성화
        fillImages[index].enabled = true;
        //슬라이더를 0에서 1로 채워주는 코루틴 실행
        StartCoroutine(ProcessIngredient(ingredient, processingTime, index));
    }

    //재료 가공 이후 재료 수량 증가
    IEnumerator ProcessIngredient(Ingredient ingredient, float time, int index)
    {
        float elapsedTime = 0f; //경과 시간
        Slider slider = progressBars[index]; //해당 재료 슬라이더

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            slider.value = Mathf.Lerp(0, 1, elapsedTime / time); //진행도 업데이트
            yield return null;
        }

        ingredient.IncreaseAmount(1); //가공 완료 후 재료의 수량 증가
        isProcessingFlags[index] = false; //가공 완료 후 플래그 초기화

        // 가공이 완료되면 슬라이더 Fill 다시 비활성화
        fillImages[index].enabled = false;
    }

    // 플레이어가 충돌 범위에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (isPanelActive)
            {
                TogglePanel(); //플레이어가 충돌 범위에서 나가면 패널 끄기
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientProcessor : MonoBehaviour
{
    [Tooltip("재료 버튼 끌어오기!")]
    public Ingredient[] ingredients;   // 설비 별 가공 할 수 있는 재료 목룍
    public Image[] ingredientIcons;    // 각 재료의 아이콘 (Fill로 진행도 표시)

    private bool isPlayerInRange;      // 플레이어 충돌 여부 (플레이어가 충돌 중일 때 버튼 누를 수 있게)
    private bool[] isProcessingFlags;    // 각 재료의 가공 중인지 여부

    private PlayerMoney playerMoney;

    void Start()
    {
        playerMoney = FindObjectOfType<PlayerMoney>();
        // 가공 상태 초기화
        isProcessingFlags = new bool[ingredients.Length];
        // 모든 재료 아이콘 초기화
        foreach (var icon in ingredientIcons)
        {
            icon.fillAmount = 1f;
        }
    }

    void Update()
    {
        //플레이어가 범위 내에 있을 때 & 가공 중이 아닐 때 => 가공 입력 처리
        if (isPlayerInRange)
        {
            if (Input.GetKeyDown(KeyCode.Z) && !isProcessingFlags[0])
            {
                if (playerMoney.money > 9)
                {
                    playerMoney.DecreaseMoney(10); //재료 가격 10원
                    StartProcessing(ingredients[0], 3.0f, 0);
                }
            }
            else if (Input.GetKeyDown(KeyCode.X) && !isProcessingFlags[1])
            {
                if (playerMoney.money > 9)
                {
                    playerMoney.DecreaseMoney(10); //재료 가격 10원
                    StartProcessing(ingredients[1], 3.0f, 1);
                }
            }
        }
    }

    private void StartProcessing(Ingredient ingredient, float processingTime, int index)
    {
        isProcessingFlags[index] = true; //가공 중 상태로 설정
        ingredientIcons[index].fillAmount = 0f; //Fill 초기화

        //가공 코루틴
        StartCoroutine(ProcessIngredient(ingredient, processingTime, index));
    }

    IEnumerator ProcessIngredient(Ingredient ingredient, float time, int index)
    {
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            ingredientIcons[index].fillAmount = Mathf.Lerp(0, 1, elapsedTime / time); //Fill 채우기
            yield return null;
        }

        ingredient.IncreaseAmount(1); //가공 완료 후 재료 1 증가
        isProcessingFlags[index] = false;
    }

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
        }
    }
}


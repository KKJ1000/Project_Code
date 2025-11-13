using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PotionCraftingPanel : MonoBehaviour
{
    [Header("이상한 혼합물 포션 프리팹")] public GameObject strangePotionPrefab;

    [Header("재료 넣는 효과음")] public AudioClip ingredientsInsertClip;

    [Header("레시피에 맞는 포션 생성 성공")] public AudioClip successPotionRecipe;

    [Header("레시피에 맞지 않는 포션 생성")] public AudioClip failPotionRecipe;

    private AudioSource audioSource; //오디오 클립 재생할 오디오 소스 컴포넌트

    public GameObject panel;                   // 조합 UI 패널
    public Button[] ingredientButtons;         // 재료 버튼 (클릭 시 슬롯에 추가)
    public Button clearButton;                 // 슬롯 비우기 버튼
    public Button getButton;                   // 포션 획득 버튼
    public Button delButton;                   // 포션 폐기 버튼
    public IngredientSlot[] ingredientSlots;   // 재료 슬롯
    public GameObject resultSlot;              // 결과 슬롯
    public Button[] createButtons;             // 끓이기, 냉장, 섞기 버튼

    public RecipeInitializer recipeInitializer; // RecipeInitializer 참조

    private List<Recipe> recipes;               // 레시피 목록
    private List<Ingredient> selectedIngredients = new List<Ingredient>(); // 선택된 재료 리스트

    private bool isPanelActive = false;   // 패널 활성화 여부
    private bool isPlayerInRange = false; // 플레이어 충돌 여부

    private GameObject createdPotion;            // 생성된 포션
    public PotionStorageSlot[] potionStorageSlots; // 포션 보관 슬롯

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        recipes = recipeInitializer.GetRecipes(); // 레시피 데이터 가져오기
        TogglePanel(false); // 시작 시 조합 패널 비활성화

        // 재료 버튼 클릭 이벤트 등록
        for (int i = 0; i < ingredientButtons.Length; i++)
        {
            int index = i; // 클로저 문제 해결을 위한 지역 변수
            ingredientButtons[i].onClick.AddListener(() => OnIngredientButtonClicked(index));
        }

        // 조합 초기화 버튼
        clearButton?.onClick.AddListener(OnClearButtonClicked);

        // 포션 타입 버튼 이벤트 등록 (끓이기, 냉장, 섞기)
        if (createButtons.Length == 3)
        {
            createButtons[0]?.onClick.AddListener(() => OnClickButtonType(PotionType.BOIL));
            createButtons[1]?.onClick.AddListener(() => OnClickButtonType(PotionType.COLD));
            createButtons[2]?.onClick.AddListener(() => OnClickButtonType(PotionType.MIX));
        }

        // 포션 획득/폐기 버튼
        getButton?.onClick.AddListener(OnClickGetButton);
        delButton?.onClick.AddListener(OnClickDelButton);
    }

    void Update()
    {
        // E 키로 패널 활성화/비활성화
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TogglePanel(!isPanelActive);
        }

        // 숫자키 입력 처리 (1~6번 키에 매핑)
        if (isPanelActive)
        {
            for (int i = 0; i < ingredientButtons.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    OnIngredientButtonClicked(i);
                }
            }
        }
    }

    // 포션 타입 버튼 클릭
    private void OnClickButtonType(PotionType type)
    {
        if (selectedIngredients.Count == ingredientSlots.Length)
        {
            Recipe matchingRecipe = GetMatchingRecipe(type);

            if (matchingRecipe != null)
            {
                if (HasEmptyStorageSlot())
                {
                    CreatePotion(matchingRecipe.resultPotionPrefab);
                    audioSource.clip = successPotionRecipe;
                    audioSource.Play();
                }
                else
                {
                    Debug.Log("포션 보관 슬롯이 가득 찼습니다.");
                }
            }
            else
            {
                if (HasEmptyStorageSlot())
                {
                    CreatePotion(strangePotionPrefab);
                    audioSource.clip = failPotionRecipe;
                    audioSource.Play();
                }
                Debug.Log("존재하지 않는 레시피입니다. \"이상한 혼합물\"이 생성되었습니다.");
            }
        }
        else
        {
            Debug.Log("모든 슬롯에 재료를 채워주세요.");
        }
    }

    // 빈 보관 슬롯 확인
    private bool HasEmptyStorageSlot()
    {
        foreach (var storageSlot in potionStorageSlots)
        {
            if (!storageSlot.IsFull) return true;
        }
        return false;
    }

    // 타입에 맞는 레시피 찾기
    private Recipe GetMatchingRecipe(PotionType type)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.potionType == type && recipe.Matches(selectedIngredients))
            {
                return recipe;
            }
        }
        return null;
    }

    // 포션 생성
    private void CreatePotion(GameObject potionPrefab)
    {
        if (potionPrefab != null)
        {
            createdPotion = Instantiate(potionPrefab, resultSlot.transform);
            Debug.Log("포션 생성 완료: " + createdPotion.name);
        }
        else
        {
            Debug.LogError("포션 프리팹이 null입니다.");
        }
    }

    // 조합 패널 활성화/비활성화
    public void TogglePanel(bool isActive)
    {
        isPanelActive = isActive;
        panel.SetActive(isPanelActive);

        if (!isPanelActive)
        {
            OnClearButtonClicked();
        }
    }

    // 재료 버튼 클릭 처리
    private void OnIngredientButtonClicked(int index)
    {
        if (!isPanelActive || index < 0 || index >= ingredientButtons.Length) return;

        Ingredient ingredient = ingredientButtons[index].GetComponent<Ingredient>();
        if (ingredient == null || ingredient.amount <= 0)
        {
            ingredient?.ShowStatus("재료가 부족합니다!");
            return;
        }

        if (selectedIngredients.Count >= ingredientSlots.Length)
        {
            ingredient.ShowStatus("슬롯이 가득 찼습니다!");
            return;
        }

        if (ingredient.DecreaseAmount(1))
        {
            audioSource.clip = ingredientsInsertClip;
            audioSource.Play();
            selectedIngredients.Add(ingredient);
            ingredientSlots[selectedIngredients.Count - 1].AddIngredient(ingredient);
        }
    }

    // 슬롯 비우기 버튼
    private void OnClearButtonClicked()
    {
        foreach (var slot in ingredientSlots)
        {
            if (!slot.IsEmpty())
            {
                Ingredient ingredient = slot.currentIngredient;
                ingredient.IncreaseAmount(1);
                selectedIngredients.Remove(ingredient);
                slot.RemoveIngredient();
            }
        }

        foreach (Transform child in resultSlot.transform)
        {
            Destroy(child.gameObject);
        }
    }

    // 포션 획득
    private void OnClickGetButton()
    {
        if (createdPotion != null && HasEmptyStorageSlot())
        {
            foreach (var storageSlot in potionStorageSlots)
            {
                if (!storageSlot.IsFull)
                {
                    storageSlot.AddPotion(createdPotion);
                    createdPotion = null;
                    break;
                }
            }
            ResetCombination();
        }
        else
        {
            Debug.Log("결과 슬롯에 포션이 없거나 보관 슬롯이 가득 찼습니다.");
        }
    }

    // 포션 폐기
    private void OnClickDelButton()
    {
        ResetCombination();
    }

    // 조합 초기화
    public void ResetCombination()
    {
        selectedIngredients.Clear();

        foreach (IngredientSlot slot in ingredientSlots)
        {
            slot.RemoveIngredient();
        }

        foreach (Transform child in resultSlot.transform)
        {
            Destroy(child.gameObject);
        }
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
            if (isPanelActive) TogglePanel(false);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ingredient : MonoBehaviour
{
    public string ingredientName; //재료 이름
    public int amount; //현재 재료 수량
    public int originalAmount; //원래 수량 (슬롯에 넣고 초기화 시켰을 때 복구 시킬 변수)
    public Text amountText; //재료 수량 표시용 텍스트
    //public Text statusText; // 상태 메시지 ("재료 수량 부족" 알림)
    public Sprite icon;     // 재료 아이콘
    public Image iconImage; // 아이콘을 표시할 UI Image 컴포넌트

    void Start()
    {
        originalAmount = amount; //현재 재료 수량을 originalAmount에 저장
        UpdateAmountText();
        UpdateIcon(); //재료 아이콘을 UI에 적용
    }

    //재료 수량 업데이트 메서드
    public void IncreaseAmount(int count)
    {
        amount += count;
        originalAmount = amount; //수량이 증가할 때 originalAmount도 갱신
        UpdateAmountText();
    }

    public bool DecreaseAmount(int count)
    {
        if (amount >= count)
        {
            amount -= count;
            originalAmount = amount; //수량이 감소할 때 originalAmount도 갱신
            UpdateAmountText();
            return true;
        }
        else
        {
            ShowStatus("재료의 수량이 부족합니다!");
            return false;
        }
    }

    // 재료 이름 : 수량 업데이트 메서드
    private void UpdateAmountText()
    {
        amountText.text = $"{ingredientName} : {amount}";
    }

    // 재료 아이콘을 UI에 적용 하는 메서드
    private void UpdateIcon()
    {
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(true);
        }
    }

    public void ShowStatus(string message)
    {
        //statusText.text = message;
        //statusText.gameObject.SetActive(true);

        // 일정 시간 후 메시지 숨김 
        Invoke(nameof(HideStatus), 1f);
    }

    private void HideStatus()
    {
        //statusText.gameObject.SetActive(false);
    }
}

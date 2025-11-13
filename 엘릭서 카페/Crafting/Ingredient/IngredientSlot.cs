using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// 포션 제작 패널의 재료 슬롯 스크립트
public class IngredientSlot : MonoBehaviour
{
    //public Image slotImage;              // 슬롯 배경 이미지
    public Image itemImage;              // 아이템 이미지 (슬롯 위에 표시될 이미지)
    public Sprite defaultSprite;         // 기본 슬롯 이미지 (슬롯 배경용)
    public Ingredient currentIngredient; // 슬롯에 들어있는 재료 정보

    void Start()
    {
        //slotImage.sprite = defaultSprite; //슬롯 배경 이미지의 스프라이트부분을 기본 슬롯 이미지로 적용
        //slotImage.enabled = true;  // 슬롯 이미지는 항상 보이게 켜두기
        itemImage.enabled = false; // 아이템 이미지는 숨기기
    }

    //재료를 슬롯에 추가하는 메서드
    public void AddIngredient(Ingredient ingredient)
    {
        currentIngredient = ingredient; // 현재 슬롯에 들어있는 정보 = 슬롯에 넣을 정보
        itemImage.sprite = ingredient.icon; // 아이템 이미지로 변경
        itemImage.enabled = true;           // 슬롯에 들어오면 아이템 이미지를 활성화
    }

    //슬롯에서 재료를 제거하는 메서드
    public void RemoveIngredient()
    {
        currentIngredient = null;  //슬롯에 들어있는 재료 정보를 null로 설정
        itemImage.enabled = false; //이미지 끄기
    }

    //현재 슬롯이 비어있는지 확인하는 메서드
    public bool IsEmpty()
    {
        return currentIngredient == null;
    }
}

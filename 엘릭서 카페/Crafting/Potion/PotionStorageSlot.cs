using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//생성된 포션 보관 슬롯
public class PotionStorageSlot : MonoBehaviour
{
    public GameObject currentPotion; //현재 슬롯에 들어있는 포션
    public bool IsFull => currentPotion != null;

    public void AddPotion(GameObject potion)
    {
        if (!IsFull) // 포션 슬롯이 가득 차지 않았다면
        {
            currentPotion = potion;
            potion.transform.SetParent(transform); //포션을 슬롯에 넣는다.

            // 포션의 로컬 위치를 조정하여 보관 슬롯 내에서 보이도록 설정
            potion.transform.localPosition = Vector3.zero;

            // 판매 슬롯과 동기화
            FindObjectOfType<PotionSalePanel>()?.UpdateSaleSlot();
        }
        else
        {
            Debug.Log("슬롯이 가득 찼습니다.");
        }
    }

    public void RemovePotion()
    {
        if (IsFull)
        {
            Destroy(currentPotion); // 포션 제거
            currentPotion = null;

            // 판매 슬롯과 동기화
            FindObjectOfType<PotionSalePanel>()?.UpdateSaleSlot();
        }
    }
}

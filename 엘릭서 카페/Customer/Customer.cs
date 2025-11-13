using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    public Sprite customerSprite;      //손님의 스프라이트 이미지
    public GameObject desiredPotion;   //손님이 원하는 포션 오브젝트 => 의 이미지만 사용할 예정

    public Image potionImage; //손님이 원하는 포션 UI (손님 오브젝트 위에 표시할 UI) // 인스펙터에서 자식 오브젝트중 Image 오브젝트 할당

    void Start()
    {
        potionImage.sprite = desiredPotion.GetComponent<Image>().sprite; //손님이 원하는 포션 UI 이미지 적용
    }
}
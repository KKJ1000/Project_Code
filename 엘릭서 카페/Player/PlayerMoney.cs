using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 추가

public class PlayerMoney : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText; // 소지금 표시용 텍스트
    [SerializeField] private TextMeshProUGUI moneyText2; // 게임 오버 시 표시
    public int money = 500; // 플레이어의 초기 돈

    void Start()
    {
        UpdateMoneyText(); // 초기 텍스트 설정
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoneyText(); // 돈 업데이트 시 텍스트 갱신
    }

    public void DecreaseMoney(int amount)
    {
        money -= amount;
        UpdateMoneyText();
    }

    private void UpdateMoneyText()
    {
        if (moneyText != null)
        {
            moneyText.text = $"소지금: {money}"; // 텍스트 갱신
        }
        else
        {
            Debug.LogError("MoneyText가 할당되지 않았습니다!");
        }

        if (moneyText2 != null)
        {
            moneyText2.text = $"최종 점수: {money}"; // 텍스트 갱신
        }
        else
        {
            Debug.LogError("moneyText2가 할당되지 않았습니다!");
        }
    }
}

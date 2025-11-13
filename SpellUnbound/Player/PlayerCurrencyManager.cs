using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerCurrencyManager : MonoBehaviour
{
    public static PlayerCurrencyManager Instance { get; private set; }

    [SerializeField] private Text m_goldText;
    [SerializeField] private Text m_gainedGoldText;
    [SerializeField] private GameObject m_gainedGoldUI;
    [SerializeField] private float mergeDelay = 2f;

    private int m_totalGold = 0;
    private int m_pendingGold = 0;
    // private float m_pendingTime = 0f;

    private Tween m_mergeTween;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateGoldUI();
    }

    // Update 방식은 비효율적
    // Why? 매 프레임마다 Update()가 호출됨
    // 대신 Couroutine 또는 DOTween을 사용하여 지연 처리 방식이 효율적
    // void Update()
    // {
    //     if (m_pendingGold > 0)
    //     {
    //         m_pendingTime += Time.deltaTime;

    //         if (m_pendingTime > mergeDelay)
    //         {
    //             MergeGold();
    //         }
    //     }
    // }

    public void AddGold(int amount)
    {
        m_pendingGold += amount;
        // m_pendingTime = 0f;

        m_gainedGoldUI.SetActive(true);
        m_gainedGoldText.text = $"+ {m_pendingGold}";

        if (m_mergeTween != null)
        {
            m_mergeTween.Kill();
        }

        m_mergeTween = DOVirtual.DelayedCall(mergeDelay, () =>
        {
            MergeGold();
            m_mergeTween = null;
        });
    }

    private void MergeGold()
    {
        m_totalGold += m_pendingGold;
        m_pendingGold = 0;

        UpdateGoldUI();
        m_gainedGoldUI.SetActive(false);
    }

    public bool TrySpendGold(int amount)
    {

        if (m_totalGold >= amount)
        {
            m_totalGold -= amount;
            UpdateGoldUI();
            return true;
        }

        return false;
    }

    public int GetCurrentGold()
    {
        return m_totalGold;
    }

    private void UpdateGoldUI()
    {
        if (m_goldText != null)
        {
            m_goldText.text = $"{m_totalGold}";
        }
    }

    public void ResetGold()
    {
        m_totalGold = 0;
        m_pendingGold = 0;
        UpdateGoldUI();
    }
}

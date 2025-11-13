using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefenseCrystal : MonoBehaviour
{
    [Header("크리스탈 스탯 설정")]
    [SerializeField] private int m_maxHp = 500;
    private int m_currentHp;

    [Header("크리스탈 UI")]
    [SerializeField] private GameObject m_gaugeUI;
    [SerializeField] private Slider m_gaugeSlider;
    [SerializeField] private GameObject m_hpBarUI;
    [SerializeField] private Slider m_hpBarSlider;

    [Header("보호방 몬스터 스폰 딜레이 설정")]
    [SerializeField] private float m_spawnDelay;

    // 게이지 설정
    private float m_gaugeDuration = 60f; // 1분
    private float m_currentGaugeTime = 0f;
    private bool m_isGaugeActive = false;

    public bool IsDestroyed => m_currentHp <= 0;

    void Start()
    {
        m_currentHp = m_maxHp;
    }

    void Update()
    {
        if (m_isGaugeActive && !IsDestroyed)
        {
            m_currentGaugeTime += Time.deltaTime;
            m_hpBarUI.transform.LookAt(Camera.main.transform);
            m_hpBarUI.transform.Rotate(0, 180f, 0);

            if (m_gaugeSlider != null)
            {
                m_gaugeSlider.value = m_currentGaugeTime;
            }

            if (m_currentGaugeTime >= m_gaugeDuration)
            {
                m_isGaugeActive = false;
                OnSurvived();
            }
        }
    }

    public void StartGauge()
    {
        m_isGaugeActive = true;
        m_currentGaugeTime = 0f;

        m_gaugeUI.SetActive(true);
        m_hpBarUI.SetActive(true);

        m_gaugeSlider.maxValue = m_gaugeDuration;
        m_gaugeSlider.value = 0f;

        m_hpBarSlider.value = (float)m_currentHp / m_maxHp;
    }

    public void TakeDamage(int damage)
    {
        if (IsDestroyed) return;

        m_currentHp -= damage;

        if (m_currentHp <= 0)
        {
            m_currentHp = 0;
            m_isGaugeActive = false;

            if (m_currentGaugeTime < m_gaugeDuration) // 60초가 다 지나가기 전에 크리스탈이 파괴 되었을 때
            {
                OnFailed();
            }
        }

        if (m_hpBarSlider != null)
        {
            m_hpBarSlider.value = (float)m_currentHp / m_maxHp;
        }
    }

    private void OnSurvived()
    {
        if (!IsDestroyed)
        {
            // TODO: 크리스탈이 버팀. 추가 보상 지급.
            if (m_gaugeUI != null) m_gaugeUI.SetActive(false);
            if (m_hpBarSlider != null) m_hpBarUI.SetActive(false);
            WaveManager.Instance.KillAliveMonsters();
            WaveManager.Instance.RoomGateInfo().OpenGate(); 
            WaveManager.Instance.CollectAllRemainingGolds();
            WaveManager.Instance.StopAllCoroutines();
            WaveManager.Instance.TrySpawnTreasureChest();
        }
    }

    private void OnFailed()
    {
        // TODO: 패배 처리 (추가 보상 없음)
        if (m_gaugeUI != null) m_gaugeUI.SetActive(false);
        if (m_hpBarSlider != null) m_hpBarUI.SetActive(false);
        WaveManager.Instance.KillAliveMonsters();
        WaveManager.Instance.RoomGateInfo().OpenGate();
        WaveManager.Instance.CollectAllRemainingGolds();
        WaveManager.Instance.StopAllCoroutines();
        WaveManager.Instance.TrySpawnTreasureChest();
        Destroy(gameObject, 1.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !m_isGaugeActive)
        {
            StartGauge();
            WaveManager.Instance.SetDefenseSpawnDelay(m_spawnDelay);
        }
    }
}

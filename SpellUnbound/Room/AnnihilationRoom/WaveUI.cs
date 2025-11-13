using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveUI : MonoBehaviour
{
    [SerializeField] private Text m_currentWaveText;
    [SerializeField, Tooltip("인스펙터 확인용")] private int m_currentWaveIndex = 0;

    private void Start()
    {
        m_currentWaveText.gameObject.SetActive(false);
    }

    public void ActiveWaveText(int currentWaveIndex)
    {
        m_currentWaveIndex = currentWaveIndex;
        m_currentWaveText.gameObject.SetActive(true);
        m_currentWaveText.text = $"Wave {m_currentWaveIndex + 1}";
        Invoke("DeactiveWaveText", 1.5f);
    }

    private void DeactiveWaveText()
    {
        m_currentWaveText.gameObject.SetActive(false);
    }
}

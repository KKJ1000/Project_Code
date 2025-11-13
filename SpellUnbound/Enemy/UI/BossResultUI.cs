using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossResultUI : MonoBehaviour
{
    [SerializeField] private GameObject bossClearUIPanel;

    private void Awake()
    {
        if (bossClearUIPanel != null)
        {
            bossClearUIPanel.SetActive(false);
        }

    }


    public void OnBossDefated()
    {
        Debug.Log("[BossStageManager] 보스 처치");

        if (bossClearUIPanel != null)
        {
            bossClearUIPanel.SetActive(true);
        }

        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.HideAllUI();
        }

    }
}

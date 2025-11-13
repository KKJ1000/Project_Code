using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveMonsterInfo
{
    public GameObject monsterPrefab;
    public Transform spawnPoint;
}

[System.Serializable]
public class WaveData
{
    public List<WaveMonsterInfo> monsterInfos = new List<WaveMonsterInfo>();
}

[System.Serializable]
public class RoomGate
{
    public GameObject gateObject;

    public void OpenGate()
    {
        if (gateObject != null)
        {
            gateObject.SetActive(false);
            Debug.Log("문이 열렸습니다.");

            SpellRewardManager.Instance.GenerateAndShowRewards(() =>
          {
              Debug.Log("[SpellRewardManager] 보상 선택 완료 or 스킵됨");
              Test5.RoomManager.Instance._isTeleportReady = true;
              PlayerInput.Instance.DisableCursor();
              PlayerUI.Instance.ShowTeleportUI();
          });
        }
    }

    public void CloseGate()
    {
        if (gateObject != null)
        {
            gateObject.SetActive(true);
            Debug.Log("웨이브 시작! 통로 차단");
        }
    }
}


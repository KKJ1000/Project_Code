using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveTrigger : MonoBehaviour
{
    [Header("충돌할 게임 오브젝트 태그")]
    [SerializeField] private string m_tagName = "Player";
    [Header("방 타입")]
    [SerializeField] private ERoomType roomType;
    [Header("방 웨이브 데이터 설정")]
    [SerializeField] private WaveData[] m_waveDatas;
    [Header("방 클리어 시 열릴 문 오브젝트")]
    [SerializeField] private RoomGate roomGate;
    
    [Header("소탕방 둥지 리스트")]
    [SerializeField] private List<GameObject> m_sweepNests = new List<GameObject>();


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(m_tagName))
        {
            if (roomType == ERoomType.Annihilation)
            {
                WaveManager.Instance.SetupRoom(roomType, m_waveDatas, roomGate);
                Destroy(gameObject);
            }
            else if (roomType == ERoomType.Defense)
            {
                WaveManager.Instance.SetDefenseCrystal(transform);
                WaveManager.Instance.SetupRoom(roomType, m_waveDatas, roomGate);
                SphereCollider sphereCollider = GetComponent<SphereCollider>();
                if (sphereCollider != null)
                {
                    sphereCollider.enabled = false;
                }
            }
            else if (roomType == ERoomType.Sweep)
            {
                WaveManager.Instance.SetupRoom(roomType, m_waveDatas, roomGate, m_sweepNests);
                Destroy(gameObject);
            }

            if (roomGate != null)
            {
                roomGate.CloseGate();
            }
        }
    }
}

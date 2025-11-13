using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseRoomTrigger : MonoBehaviour
{
    [SerializeField] private RoomGate roomGate;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            roomGate.CloseGate();
            WaveManager.Instance.SetRoomGate(roomGate);
            Destroy(gameObject);
        }
    }
}

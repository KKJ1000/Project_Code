using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    public Transform destinationPortal; //이동 할 포탈
    private bool isPlayerInRange; //플레이어가 포탈 근처에 있는 여부
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
    
    void Update()
    {
        if (isPlayerInRange && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
        {
            TeleportPlayer();
        }
    }

    void TeleportPlayer()
    {
        if (destinationPortal != null)
        {
            // 플레이어 위치를 목적지 포탈 위치로 이동
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = destinationPortal.position;
            }
        }
    }
   
}

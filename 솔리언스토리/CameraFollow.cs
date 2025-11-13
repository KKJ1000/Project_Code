using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; //플레이어 위치
    public Vector3 offset; //플레이어와 카메라 사이 거리

    private void Update()
    {
        Vector3 playerPosition = player.position + offset;
        transform.position = new Vector3(playerPosition.x, playerPosition.y, transform.position.z);
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시작 지점에서 넘어가는 문에 설치
/// </summary>
public class TutorialTrigger : MonoBehaviour
{
    public bool IsTriggered { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsTriggered = true;
            GetComponent<Collider>().enabled = false;
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTutorial : MonoBehaviour
{
    [Header("튜토리얼 종료 후 이동 씬 이름")]
    [SerializeField] private string sceneName = "";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Collider>().enabled = false;
            FadeManager.Instance.StartFadeOut(1.5f, sceneName);
        }
    }
}

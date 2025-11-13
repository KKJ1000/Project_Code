using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnClickStartBtn()
    {
        SceneManager.LoadScene(2);
    }

    public void OnClickQuitBtn()
    {
        Application.Quit();
        Debug.Log("게임 나가기");
    }
}

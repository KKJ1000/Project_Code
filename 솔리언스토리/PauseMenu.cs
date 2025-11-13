using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject warningUI;

    public bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        warningUI.SetActive(false);
    }

    void Update()
    {
        if(!isPaused && Input.GetKeyDown(KeyCode.Escape))//퍼즈상태가 아니고 ESC를 누르면 퍼즈 
        {
            Pause();
        }
    }

    public void Resume() //게임 재개 함수
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // 게임 시간을 다시 시작
        isPaused = false;
    }

    void Pause() //게임 정지 함수
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // 게임 시간을 멈춤
        isPaused = true;
    }

    public void LoadWarningUI() //퍼즈 메뉴에서 메인메뉴 버튼 클릭 시 호출 될 함수 / 경고창 활성화
    {
        pauseMenuUI.SetActive(false);
        warningUI.SetActive(true);
    }

    public void LoadPauseMenu() //경고창에서 아니요를 클릭 했을 때 호출 될 함수
    {
        //다시 퍼즈 메뉴로 돌아오는 함수
        pauseMenuUI.SetActive(true);
        warningUI.SetActive(false);
    }
    public void LoadMainMenu() //퍼즈 메뉴에서 메인메뉴 버튼 누른 뒤 예를 눌렀을 때 호출 될 함수 / 메인 메뉴 로드 씬
    {
        Time.timeScale = 1f; //씬 전환 하기 전 멈춰놨던 게임 시간을 다시 시작
        SceneManager.LoadScene("01.MainMenu");
    }
}

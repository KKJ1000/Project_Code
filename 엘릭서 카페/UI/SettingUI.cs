using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//인게임 내에서 Pause시 설정 창
public class SettingUI : MonoBehaviour
{
    [SerializeField]
    private GameObject settingPanel; //설정 메뉴 UI
    [SerializeField]
    private Button settingBtn;       //설정 메뉴 활성화 버튼
    [SerializeField]
    private Button continueBtn;      //계속하기 버튼
    [SerializeField]
    private Button mainMenuBtn;      //메인메뉴 버튼

    private void Awake()
    {
        Time.timeScale = 1f;
        settingPanel.SetActive(false);
        settingBtn.onClick.AddListener(OnSettingPanelBtn);
        continueBtn.onClick.AddListener(OnContinueBtn);
        mainMenuBtn.onClick.AddListener(GoMainMenuBtn);
    }

    //설정패널 활성화
    private void OnSettingPanelBtn()
    {
        StartCoroutine(SettingPanelCoroutine());
    }

    //설정패널 활성화 코루틴
    private IEnumerator SettingPanelCoroutine()
    {
        settingPanel.SetActive(true);
        // TimeScale 버그
        Time.timeScale = 0f;
        yield return null;
    }

    //계속하기 버튼 
    private void OnContinueBtn()
    {
        StartCoroutine(ContinueCoroutine());
    }
    //계속하기 버튼 코루틴
    private IEnumerator ContinueCoroutine()
    {
        settingPanel.SetActive(false);
        Time.timeScale = 1f;
        yield return null;
    }

    private void GoMainMenuBtn()
    {
        StartCoroutine(MainMenuCoroutine());
    }

    private IEnumerator MainMenuCoroutine()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        yield return null;
    }


}

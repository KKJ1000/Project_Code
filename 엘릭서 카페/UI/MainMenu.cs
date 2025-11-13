using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("메인 버튼")]
    [SerializeField] private Button openStartPanel;    // 게임 시작 버튼
    [SerializeField] private Button openContinuePanel; // 계속 하기 버튼
    [SerializeField] private Button openSettingPanel;  // 설정 버튼

    [Header("서브 버튼")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button startChallengeButton;
    [SerializeField] private Button startPanelCloseButton;
    [SerializeField] private Button continuePanelCloseButton;
    [SerializeField] private Button settingPanelCloseButton;

    [Header("UI Panel")]
    [SerializeField] private GameObject startPanel;    // 게임 시작 패널
    [SerializeField] private GameObject continuePanel; // 계속 하기 패널
    [SerializeField] private GameObject settingPanel;  // 설정 패널


    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        startChallengeButton.onClick.AddListener(StartChallengeGame);
        openStartPanel.onClick.AddListener(() => ActivePanel(0));
        openContinuePanel.onClick.AddListener(( )=> ActivePanel(1));
        openSettingPanel.onClick.AddListener(() => ActivePanel(2));
        startPanelCloseButton.onClick.AddListener(() => DeActivePanel(0));
        continuePanelCloseButton.onClick.AddListener(() => DeActivePanel(1));
        settingPanelCloseButton.onClick.AddListener(() => DeActivePanel(2));
    }

    private void StartGame() // 시작 패널의 게임시작 버튼에 적용
    {
        SceneManager.LoadScene(1);
    }

    private void StartChallengeGame() // 시작 패널의 게임시작 버튼에 적용
    {
        SceneManager.LoadScene(3); //게임 씬
    }

    private void ActivePanel(int index)
    {
        if (index == 0) startPanel.SetActive(true);
        else if (index == 1) continuePanel.SetActive(true);
        else settingPanel.SetActive(true);
    }
    
    private void DeActivePanel(int index)
    {
        if (index == 0) startPanel.SetActive(false);
        else if(index == 1) continuePanel.SetActive(false);
        else settingPanel.SetActive(false);
    }
//    public void QuitGame()  // 게임 종료 버튼에 적용
//    {
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//        Application.Quit(); // 어플리케이션 종료
//#endif
//    }
}

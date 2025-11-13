using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [Header("설정 패널들")]
    [SerializeField] private GameObject generalPanel;
    [SerializeField] private GameObject displayPanel;
    [SerializeField] private GameObject soundPanel;

    [Header("버튼")]
    [SerializeField] private Button generalButton;
    [SerializeField] private Button displayButton;
    [SerializeField] private Button soundButton;

    [Header("UI 요소")]
    [SerializeField] private Text titleText; // 설정 제목 표시 텍스트
    [SerializeField] private Toggle muteToggle; //음소거 설정 토글

    // 선택된 버튼과 비선택된 버튼의 색상 설정
    private Color selectedButtonColor = Color.black;
    private Color defaultButtonColor = new Color(217f / 255f, 217f / 255f, 217f / 255f); //RGB(217,217,217)
    private Color selectedTextColor = Color.white;
    private Color defaultTextColor = Color.black;
    
    void Start()
    {
        generalButton.onClick.AddListener(() => ShowPanel("General"));
        displayButton.onClick.AddListener(() => ShowPanel("Display"));
        soundButton.onClick.AddListener(() => ShowPanel("Sound"));

        // 일반 설정 패널을 기본값
        ShowPanel("General");

        // 저장된 음소거 상태를 불러와 토글 버튼에 반영
        bool isMuted = PlayerPrefs.GetInt("isMuted", 0) == 1;
        muteToggle.isOn = isMuted;

        // 토글 상태 변경 시 BGMManager의 SetMUte호출
        muteToggle.onValueChanged.AddListener(isOn => BGMManager.Instance.SetMute(isOn)); //음소거 토글 이벤트
    }

    // 설정 패널 및 버튼 색상 업데이트
    private void ShowPanel(string panelType)
    {
        //모든 패널 비활성화하고 특정 패널만 활성화
        generalPanel.SetActive(panelType == "General");
        displayPanel.SetActive(panelType == "Display");
        soundPanel.SetActive(panelType == "Sound");

        // Title Text를 패널에 맞게 변경
        switch (panelType)
        {
            case "General":
                titleText.text = "일반 설정";
                UpdateButtonColors(generalButton);
                displayPanel.SetActive(false);
                soundPanel.SetActive(false);
                break;
            case "Display":
                titleText.text = "화면 설정";
                UpdateButtonColors(displayButton);
                generalPanel.SetActive(false);
                soundPanel.SetActive(false);
                break;
            case "Sound":
                titleText.text = "소리 설정";
                UpdateButtonColors(soundButton);
                generalPanel.SetActive(false);
                displayPanel.SetActive(false);
                break;
        }
    }

    // 선택한 버튼 색상과 텍스트 색상 변경
    private void UpdateButtonColors(Button selectedButton)
    {
        // 모든 버튼을 기본 색상으로 설정
        ResetButtonColors();

        // 선택된 버튼 색상 설정
        selectedButton.GetComponent<Image>().color = selectedButtonColor;
        selectedButton.GetComponentInChildren<Text>().color = selectedTextColor;
    }

    //모든 버튼을 기본 색상으로 초기화하는 메서드
    private void ResetButtonColors()
    {
        // 모든 버튼을 디폴트 상태로 설정
        generalButton.GetComponent<Image>().color = defaultButtonColor;
        generalButton.GetComponentInChildren<Text>().color = defaultTextColor;

        displayButton.GetComponent<Image>().color = defaultButtonColor;
        displayButton.GetComponentInChildren<Text>().color = defaultTextColor;

        soundButton.GetComponent<Image>().color = defaultButtonColor;
        soundButton.GetComponentInChildren<Text>().color = defaultTextColor;
    }

}

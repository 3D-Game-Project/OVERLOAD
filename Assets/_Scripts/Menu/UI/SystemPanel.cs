using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class SystemPanel : MonoBehaviour
{
    [Header("패널 연결")]
    [Tooltip("미리 씬에 배치해두고 꺼둔 Guide Panel 오브젝트를 넣으세요.")]
    [SerializeField] private GameObject _guidePanel;

    [Header("버튼 연결")]
    [SerializeField] private Button _guideButton;
    [SerializeField] private Button _mainMenuButton;

    [Header("씬 설정")]
    [SerializeField] private string _titleSceneName = "TitleScene"; 

    private void Start()
    {
        if (_guidePanel != null)
        {
            _guidePanel.SetActive(false);
        }

        if (_guideButton != null)
        {
            _guideButton.onClick.AddListener(OnClickGuideButton);
        }

        if (_mainMenuButton != null)
        {
            _mainMenuButton.onClick.AddListener(OnClickMainMenuButton);
        }
    }

    private void OnClickGuideButton()
    {
        if (_guidePanel != null)
        {
            _guidePanel.SetActive(true);
        }
    }

    public void CloseGuidePanel()
    {
        if (_guidePanel != null)
        {
            _guidePanel.SetActive(false);
        }
    }

    private void OnClickMainMenuButton()
    {
        Debug.Log($"[SystemPanel] {_titleSceneName} 씬으로 이동합니다.");

        Time.timeScale = 1f;

        SceneManager.LoadScene(_titleSceneName);
    }

    private void OnDestroy()
    {
        if (_guideButton != null) _guideButton.onClick.RemoveListener(OnClickGuideButton);
        if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveListener(OnClickMainMenuButton);
    }
}
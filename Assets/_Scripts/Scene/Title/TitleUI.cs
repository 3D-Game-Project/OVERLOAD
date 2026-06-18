using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private Button _guide;
    [SerializeField] private Button _play;
    [SerializeField] private Button _settings;

    [SerializeField] private GameObject _guidePanel;
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private GameObject _settingPanel;

    private void Awake()
    {
        if (_play != null) _play.onClick.AddListener(OnClickPlay);
        if (_guide != null) _guide.onClick.AddListener(OnClickGuide);
        if (_settings != null) _settings.onClick.AddListener(OnClickSettings);

        InitPanels();
    }

    private void InitPanels()
    {
        if (_guidePanel != null) _guidePanel.SetActive(false);
        if (_settingPanel != null) _settingPanel.SetActive(false);
        if (_loadingPanel != null) _loadingPanel.SetActive(false);
    }

    private void OnClickPlay()
    {
        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(true);
        }
    }

    private void OnClickGuide()
    {
        if (_guidePanel != null)
        {
            _guidePanel.SetActive(true);
        }
    }

    private void OnClickSettings()
    {
        if (_settingPanel != null)
        {
            _settingPanel.SetActive(true);
        }
    }
}

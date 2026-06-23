using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PlayerDeathUI : MonoBehaviour
{
    public static PlayerDeathUI Instance { get; private set; }

    [Header("UI 구성 요소")]
    [SerializeField] private GameObject _uiRoot;
    [SerializeField] private Image _glitchImage;
    [SerializeField] private Image _blackFadeImage;

    [Header("버튼 개별 연결")]
    [SerializeField] private Button _respawnButton;
    [SerializeField] private Button _mainMenuButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (_uiRoot != null) _uiRoot.SetActive(false);

        if (_respawnButton != null) _respawnButton.onClick.AddListener(OnClickRespawn);
        if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(OnClickMainMenu);
    }

    public void PlayGlitchEffect()
    {
        if (_uiRoot != null) _uiRoot.SetActive(true);

        if (_respawnButton != null) _respawnButton.gameObject.SetActive(false);
        if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(false);

        if (_blackFadeImage != null)
        {
            _blackFadeImage.DOKill(); 
            _blackFadeImage.gameObject.SetActive(false);
        }

        if (_glitchImage != null)
        {
            _glitchImage.gameObject.SetActive(true);
            _glitchImage.color = new Color(1f, 1f, 1f, 1f);
            _glitchImage.DOFade(0.5f, 0.05f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void StartBlackFade(float duration)
    {
        if (_glitchImage != null)
        {
            _glitchImage.DOKill();
            _glitchImage.gameObject.SetActive(false);
        }

        if (_blackFadeImage != null)
        {
            _blackFadeImage.gameObject.SetActive(true);
            _blackFadeImage.color = new Color(0, 0, 0, 0); 

            _blackFadeImage.DOFade(1f, duration).OnComplete(() =>
            {
                ShowDeathButtons();
            });
        }
    }

    private void ShowDeathButtons()
    {
        if (_respawnButton != null) _respawnButton.gameObject.SetActive(true);
        if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnClickRespawn()
    {
        if (_uiRoot != null) _uiRoot.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (_blackFadeImage != null)
        {
            _blackFadeImage.gameObject.SetActive(false);
            _blackFadeImage.color = new Color(0, 0, 0, 0);
        }

        if (_glitchImage != null)
        {
            _glitchImage.DOKill();
            _glitchImage.gameObject.SetActive(false);
        }


        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && player.TryGetComponent(out DurabilityController core))
        {
            core.RespawnNearPoint();
        }
    }

    private void OnClickMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
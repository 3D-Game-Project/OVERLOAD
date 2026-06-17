using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MenuController : MonoBehaviour
{
    private PlayerInputHandler _inputHandler;
    private Shop shop;

    [Header("최상위 이미지, 패널")]
    [SerializeField] private GameObject backgroundImage;
    [SerializeField] private GameObject mainInterfacePanel;

    [Header("메뉴: 스탯, 상점, 인벤, 시스템순서 고정")]
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject systemSettings;

    [Header("상단 네비게이션 버튼")]
    [SerializeField] private Button previousBtn;
    [SerializeField] private Button nextBtn;

    [Header("Zelda Style 3개 탭 텍스트 참조")]
    [SerializeField] private TextMeshProUGUI _leftTabText;    
    [SerializeField] private TextMeshProUGUI _currentTabText;  
    [SerializeField] private TextMeshProUGUI _rightTabText;   

    [Header("탭 이름 정의 (순서 일치 필수)")]
    [SerializeField] private string[] tabNames = { "Character", "Shop", "Inventory", "Settings" };

    [SerializeField] private GameObject playerPrefab;
    private CanvasGroup[] _tabCanvasGroups;
    private CanvasGroup _mainInterfaceCanvasGroup;

    private GameObject[] _tabPanels;
    private int _currentTabIndex = 0;
    private bool _isMenuOpen = false;

    // 팝업끄기용으로 추가
    private SlotOptionPopupUI _slotOptionPopup;

    public bool IsShop { get; set; } = false;

    public int CurrentTabIndex => _currentTabIndex;
    public bool IsMenuOpen => _isMenuOpen;

    // 버튼 연결 및 패널에 순서 매핑
    private void Awake()
    {
        _tabPanels = new GameObject[] { statusPanel, shopPanel, inventoryPanel, systemSettings };

        _tabCanvasGroups = new CanvasGroup[_tabPanels.Length];

        if (mainInterfacePanel != null)
        {
            _mainInterfaceCanvasGroup = mainInterfacePanel.GetComponent<CanvasGroup>();
            if (_mainInterfaceCanvasGroup == null)
            {
                _mainInterfaceCanvasGroup = mainInterfacePanel.AddComponent<CanvasGroup>();
            }
            _mainInterfaceCanvasGroup.alpha = 0f;
            _mainInterfaceCanvasGroup.blocksRaycasts = false;
        }

        if (previousBtn != null) previousBtn.onClick.AddListener(NavigateToPreviousTab);
        if (nextBtn != null) nextBtn.onClick.AddListener(NavigateToNextTab);
        if (shop == null) shop = FindFirstObjectByType<Shop>();

        _slotOptionPopup = FindFirstObjectByType<SlotOptionPopupUI>(FindObjectsInactive.Include);

        for (int i = 0; i < _tabPanels.Length; i++)
        {
            if (_tabPanels[i] != null)
            {
                _tabCanvasGroups[i] = _tabPanels[i].GetComponent<CanvasGroup>();
                if (_tabCanvasGroups[i] == null)
                {
                    _tabCanvasGroups[i] = _tabPanels[i].AddComponent<CanvasGroup>();
                }

                _tabCanvasGroups[i].alpha = 0f;
                _tabCanvasGroups[i].blocksRaycasts = false;
                _tabPanels[i].SetActive(false);
            }
        }
    }

    private void Start()
    {
        _inputHandler = FindFirstObjectByType<PlayerInputHandler>();
        CloseMenu();
    }

    private void Update()
    {
        if (_inputHandler == null) return;

        HandleKeyInputs();
    }

    // 키 입력에 따라 메뉴 인터페이스 열기 및 최우선 표시되어야하는 패널 처리
    private void HandleKeyInputs()
    {
        // 인벤토리
        if (_inputHandler.InventoryTriggered)
        {
            _inputHandler.InventoryTriggered = false; 

            if (!_isMenuOpen) OpenMenu(2); 
            else if (_currentTabIndex == 2) CloseMenu(); 
            else SwitchTab(2); 
        }

        // 시스템 설정
        if (_inputHandler.MenuTriggered)
        {
            _inputHandler.MenuTriggered = false;

            if (_isMenuOpen && _slotOptionPopup != null && _slotOptionPopup.gameObject.activeSelf)
            {
                _slotOptionPopup.Close();
                return;
            }

            if (!_isMenuOpen) OpenMenu(3); 
            else CloseMenu(); 
        }

        // 상점
        if (_inputHandler.InteractTriggered)
        {
            _inputHandler.InteractTriggered = false;

            if (!_isMenuOpen)
            {
                if (IsShop)
                {
                    shop.AddShopList();
                    OpenMenu(1);
                }
            }
            else if (_currentTabIndex == 1)
            {
                CloseMenu();
            }
            else
            {
                if (IsShop) SwitchTab(1);
            }
        }
    }

    // 메뉴 열기
    // 이미지와 패널 활성화시키기
    // 패널 활성화와 동시에 캐릭터 정면 프리뷰생성
    public void OpenMenu(int targetTabIdx)
    {
        _isMenuOpen = true;
        if (backgroundImage != null) backgroundImage.SetActive(true);
        if (mainInterfacePanel != null) mainInterfacePanel.SetActive(true);

        GameObject playerRootObj = GameObject.Find("Crosshair");
        if (playerRootObj != null) playerRootObj.SetActive(false);

        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;

        if (MechPreviewStudio.Instance != null && playerPrefab != null)
        {
            MechPreviewStudio.Instance.SetupPreviewModel(playerPrefab);
        }

        if (_mainInterfaceCanvasGroup != null)
        {
            _mainInterfaceCanvasGroup.DOKill();
            _mainInterfaceCanvasGroup.blocksRaycasts = true;
            _mainInterfaceCanvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic);
        }

        SetAllDurabilityUIActive(false);

        _currentTabIndex = targetTabIdx;
        SwitchTab(targetTabIdx);
    }

    // 메뉴 닫기
    // 이미지, 패널 비활성화시키기
    // 캐릭터 정면뷰 제거처리
    public void CloseMenu()
    {
        _isMenuOpen = false;
        if (backgroundImage != null) backgroundImage.SetActive(false);
        if (mainInterfacePanel != null) mainInterfacePanel.SetActive(false);

        GameObject canvasObj = GameObject.Find("Canvas"); 

        if (canvasObj != null)
        {
            Transform crosshairTransform = canvasObj.transform.Find("Crosshair");

            if (crosshairTransform != null)
            {
                crosshairTransform.gameObject.SetActive(true);
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SellPopup[] sellPopups = FindObjectsByType<SellPopup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var popup in sellPopups)
        {
            popup.gameObject.SetActive(false);
        }

        if (_slotOptionPopup != null)
        {
            _slotOptionPopup.Close();
        }

        PartDetailPopup[] detailPopups = FindObjectsByType<PartDetailPopup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var popup in detailPopups)
        {
            popup.gameObject.SetActive(false);
        }

        if (MechPreviewStudio.Instance != null)
        {
            MechPreviewStudio.Instance.CleanUpPreview();
        }

        if (_mainInterfaceCanvasGroup != null)
        {
            _mainInterfaceCanvasGroup.DOKill();
            _mainInterfaceCanvasGroup.blocksRaycasts = false;

            _mainInterfaceCanvasGroup.DOFade(0f, 0.15f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                if (mainInterfacePanel != null) mainInterfacePanel.SetActive(false);
                if (backgroundImage != null) backgroundImage.SetActive(false);

                for (int i = 0; i < _tabPanels.Length; i++)
                {
                    if (_tabPanels[i] != null && _tabCanvasGroups[i] != null)
                    {
                        _tabCanvasGroups[i].DOKill();
                        _tabCanvasGroups[i].alpha = 0f;
                        _tabCanvasGroups[i].blocksRaycasts = false;
                        _tabPanels[i].SetActive(false);
                    }
                }
            });
        }

        SetAllDurabilityUIActive(true);
    }

    // 탭 전환처리
    // 인덱스에 맞는 패널만 켜고 나머지 끄기
    private void SwitchTab(int index)
    {
        int previousTabIndex = _currentTabIndex;
        _currentTabIndex = index;

        if (previousTabIndex != _currentTabIndex)
        {
            CanvasGroup oldGroup = _tabCanvasGroups[previousTabIndex];
            GameObject oldPanel = _tabPanels[previousTabIndex];

            if (oldGroup != null && oldPanel != null)
            {
                oldGroup.DOKill();
                oldGroup.blocksRaycasts = false;
                oldGroup.DOFade(0f, 0.08f).SetEase(Ease.OutQuad).OnComplete(() => {
                    oldPanel.SetActive(false);
                });
            }
        }

        for (int i = 0; i < _tabPanels.Length; i++)
        {
            if (_tabCanvasGroups[i] == null) continue;

            if (i == _currentTabIndex)
            {
                _tabCanvasGroups[i].DOKill();
                _tabCanvasGroups[i].blocksRaycasts = true;

                _tabPanels[i].SetActive(true);
                _tabCanvasGroups[i].blocksRaycasts = true;

                _tabCanvasGroups[i].DOFade(1f, 0.12f).SetEase(Ease.InQuad);
            }
        }
        UpdateTabNavigationTexts();
    }

    // 탭전환 (다음탭)
    // 인덱스 증가시키고, 배열 길이로 나눈 나머지로 순환하도록처리하여 마지막인 시스템 설정에서 다시 스탯으로 넘어가도록 처리
    // 상점이 활성화되지않는다면 상점탭은 스킵되도록 처리
    private void NavigateToNextTab()
    {
        int nextIdx = (_currentTabIndex + 1) % _tabPanels.Length;

        if (nextIdx == 1 && !IsShop)
        {
            nextIdx = (nextIdx + 1) % _tabPanels.Length;
        }

        SwitchTab(nextIdx);
    }

    // 탭전환 (이전탭)
    // 인덱스 감소시키고, 배열 길이로 나눈 나머지로 순환하도록처리하여 첫번째인 스탯에서 다시 시스템 설정으로 넘어가도록 처리
    // 상점이 활성화되지않는다면 상점탭은 스킵되도록 처리
    private void NavigateToPreviousTab()
    {
        int prevIdx = (_currentTabIndex - 1 + _tabPanels.Length) % _tabPanels.Length;

        if (prevIdx == 1 && !IsShop)
        {
            prevIdx = (prevIdx - 1 + _tabPanels.Length) % _tabPanels.Length;
        }

        SwitchTab(prevIdx);
    }

    // 현재 탭에 따른 텍스트 변경 함수
    private void UpdateTabNavigationTexts()
    {
        if (tabNames == null || tabNames.Length == 0) return;

        if (_currentTabText != null) _currentTabText.text = tabNames[_currentTabIndex];

        // [좌측] 이전 탭 인덱스 계산 (상점 조건부 스킵 반영)
        int previousIdx = (_currentTabIndex - 1 + _tabPanels.Length) % _tabPanels.Length;
        if (previousIdx == 1 && !IsShop)
        {
            previousIdx = (previousIdx - 1 + _tabPanels.Length) % _tabPanels.Length;
        }
        if (_leftTabText != null) _leftTabText.text = tabNames[previousIdx];

        // [우측] 다음 탭 인덱스 계산 (상점 조건부 스킵 반영)
        int nextIdx = (_currentTabIndex + 1) % _tabPanels.Length;
        if (nextIdx == 1 && !IsShop)
        {
            nextIdx = (nextIdx + 1) % _tabPanels.Length;
        }
        if (_rightTabText != null) _rightTabText.text = tabNames[nextIdx];
    }

    private void SetAllDurabilityUIActive(bool isActive)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform transform in allTransforms)
        {
            if (transform.name.Contains("Durability") && transform.gameObject.scene.name != null)
            {
                transform.gameObject.SetActive(isActive);
            }
        }
    }

}

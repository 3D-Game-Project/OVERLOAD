using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private PlayerInputHandler _inputHandler;

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

    private GameObject[] _tabPanels;
    private int _currentTabIndex = 0;
    private bool _isMenuOpen = false;

    public bool IsShop { get; set; } = false;

    // 버튼 연결 및 패널에 순서 매핑
    private void Awake()
    {
        _tabPanels = new GameObject[] { statusPanel, shopPanel, inventoryPanel, systemSettings };

        if (previousBtn != null) previousBtn.onClick.AddListener(NavigateToPreviousTab);
        if (nextBtn != null) nextBtn.onClick.AddListener(NavigateToNextTab);
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

            if (!_isMenuOpen) OpenMenu(3); 
            else CloseMenu(); 
        }

        // 상점
        if (_inputHandler.InteractTriggered)
        {
            _inputHandler.InteractTriggered = false;

            if (IsShop && !_isMenuOpen)
            {
                OpenMenu(1);
            }
        }
    }

    // 메뉴 열기
    // 이미지와 패널 활성화시키기
    public void OpenMenu(int targetTabIdx)
    {
        _isMenuOpen = true;
        if (backgroundImage != null) backgroundImage.SetActive(true);
        if (mainInterfacePanel != null) mainInterfacePanel.SetActive(true);

        SwitchTab(targetTabIdx);
    }

    // 메뉴 닫기
    // 이미지, 패널 비활성화시키기
    public void CloseMenu()
    {
        _isMenuOpen = false;
        if (backgroundImage != null) backgroundImage.SetActive(false);
        if (mainInterfacePanel != null) mainInterfacePanel.SetActive(false);
    }

    // 탭 전환처리
    // 인덱스에 맞는 패널만 켜고 나머지 끄기
    private void SwitchTab(int index)
    {
        _currentTabIndex = index;

        for (int i = 0; i < _tabPanels.Length; i++)
        {
            if (_tabPanels[i] != null)
            {
                _tabPanels[i].SetActive(i == _currentTabIndex);
            }
        }
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

}

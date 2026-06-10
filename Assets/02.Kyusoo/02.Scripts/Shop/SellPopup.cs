using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellPopup : MonoBehaviour
{
    [Header("UI 컴포넌트 참조")]
    [SerializeField] private TextMeshProUGUI _noticeText;      
    [SerializeField] private TextMeshProUGUI _targetNameText;  
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    private PartsData _tempPart;
    private Shop _shop;
    private PlayerInventory _inventory;

    private void Awake()
    {
        if (_yesButton != null) _yesButton.onClick.AddListener(ExecuteSell);
        if (_noButton != null) _noButton.onClick.AddListener(ClosePopup);

    }

    // 판매 팝업창 열기
    public void OpenPopup(PartsData part, Shop shop, PlayerInventory inventory)
    {
        if (part == null || shop == null || inventory == null) return;

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        _tempPart = part;
        _shop = shop;
        _inventory = inventory;

        if (_noticeText != null) _noticeText.text = "Do you want to sell?";
        if (_targetNameText != null) _targetNameText.text = $"Part: <color=#FFCC00>{part.PartsName}</color>";

        gameObject.SetActive(true);
    }

    // Y버튼 눌렀을 때 동작할 함수
    private void ExecuteSell()
    {
        if (_shop != null && _tempPart != null && _inventory != null)
        {
            _shop.SellPart(_tempPart, _inventory);
        }

        ClosePopup();
    }

    // 팝업창 닫기
    private void ClosePopup()
    {
        _tempPart = null;
        _shop = null;
        _inventory = null;
        gameObject.SetActive(false);
    }
}
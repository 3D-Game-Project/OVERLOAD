using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SellPopup : MonoBehaviour
{
    [Header("UI 컴포넌트 참조")]
    [SerializeField] private TextMeshProUGUI _noticeText;      
    [SerializeField] private TextMeshProUGUI _targetNameText;  
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    private InventoryPartItem _tempPartItem;
    private Shop _shop;
    private PlayerInventory _inventory;

    private void Awake()
    {
        if (_yesButton != null) _yesButton.onClick.AddListener(ExecuteSell);
        if (_noButton != null) _noButton.onClick.AddListener(ClosePopup);

    }

    //// 판매 팝업창 열기
    //public void OpenPopup(PartsData part, Shop shop, PlayerInventory inventory)
    //{
    //    if (part == null || shop == null || inventory == null) return;

    //    if (UnityEngine.EventSystems.EventSystem.current != null)
    //    {
    //        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    //    }

    //    _tempPart = part;
    //    _shop = shop;
    //    _inventory = inventory;

    //    if (_noticeText != null) _noticeText.text = "Do you want to sell?";
    //    if (_targetNameText != null) _targetNameText.text = $"Part: <color=#FFCC00>{part.PartsName}</color>";

    //    gameObject.SetActive(true);
    //}

    public void OpenPopup(InventoryPartItem partItem, Shop shop, PlayerInventory inventory)
    {
        if (partItem == null ||
            partItem.PartsData == null ||
            shop == null ||
            inventory == null)
        {
            return;
        }

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        _tempPartItem = partItem;
        _shop = shop;
        _inventory = inventory;

        PartsData part = partItem.PartsData;
        CurrencyCost reward = shop.GetSellReward(part);

        if (_noticeText != null)
        {
            _noticeText.text =
                $"Sell for Gear {reward.Gear} / Scrap {reward.Scrap}?";
        }

        if (_targetNameText != null)
        {
            _targetNameText.text =
                $"Part: <color=#FFCC00>{part.PartsName}</color>";
        }

        gameObject.SetActive(true);
    }


    private void ExecuteSell()
    {
        if (_shop != null &&
            _tempPartItem != null &&
            _inventory != null)
        {
            _shop.SellPart(_tempPartItem, _inventory);
        }

        ClosePopup();
    }

    private void ClosePopup()
    {
        _tempPartItem = null;
        _shop = null;
        _inventory = null;

        gameObject.SetActive(false);
    }
}
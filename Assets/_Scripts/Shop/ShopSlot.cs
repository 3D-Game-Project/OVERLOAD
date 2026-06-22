using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("TextMeshPro 및 UI 컴포넌트 참조")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Button _buyButton;

    [Header("Gear Price")]
    [SerializeField] private GameObject _gearPriceArea;
    [SerializeField] private TextMeshProUGUI _gearPriceText;

    [Header("Scrap Price")]
    [SerializeField] private GameObject _scrapPriceArea;
    [SerializeField] private TextMeshProUGUI _scrapPriceText;

    private PartsData _originPart;
    private ItemData _originItem;
    private Shop _shop;
    private PlayerInventory _playerInventory;

    private void Awake()
    {
        if (_buyButton != null)
        {
            _buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
    }

    // 파츠 정보 설정
    public void SetupSlot(PartsData part, Shop shop, PlayerInventory inventory)
    {
        _originPart = part;
        _originItem = null; 
        _shop = shop;
        _playerInventory = inventory;

        if (part == null) return;

        if (_nameText != null) _nameText.text = part.PartsName;
        if (_descText != null) _descText.text = part.Description;

        UpdatePrice(part.BuyCost);
        UpdateShopIcon(part.PartsImage);
    }

    // 아이템 정보 설정
    public void SetupSlot(ItemData item, Shop shop, PlayerInventory inventory)
    {
        _originItem = item;
        _originPart = null; 
        _shop = shop;
        _playerInventory = inventory;

        if (item == null) return;

        if (_nameText != null) _nameText.text = item.ItemName;
        if (_descText != null) _descText.text = item.Description;

        UpdatePrice(item.BuyCost);
        UpdateShopIcon(item.ItemImage);
    }

    private void UpdatePrice(CurrencyCost cost)
    {
        SetPriceUI(_gearPriceArea, _gearPriceText, cost.Gear);
        SetPriceUI(_scrapPriceArea, _scrapPriceText, cost.Scrap);
    }

    private void SetPriceUI(
    GameObject priceArea,
    TextMeshProUGUI priceText,
    int amount)
    {
        bool hasCost = amount > 0;

        if (priceArea != null)
            priceArea.SetActive(hasCost);

        if (priceText != null)
            priceText.text = amount.ToString();
    }

    private void UpdateShopIcon(Sprite targetSprite)
    {
        if (_iconImage == null)
            return;

        _iconImage.sprite = targetSprite;
        _iconImage.color =
            targetSprite != null ? Color.white : Color.black;
    }

    //// List에 표시할 이미지 설정
    //private void UpdateShopList(Sprite targetSprite)
    //{
    //    if (_iconImage == null) return;

    //    if (targetSprite != null)
    //    {
    //        _iconImage.gameObject.SetActive(true);
    //        _iconImage.sprite = targetSprite;
    //        _iconImage.color = Color.white; 
    //    }
    //    else
    //    {
    //        _iconImage.gameObject.SetActive(true);
    //        _iconImage.sprite = null;
    //        _iconImage.color = Color.black;
    //    }
    //}

    private void OnBuyButtonClicked()
    {
        if (_shop == null || _playerInventory == null) return;

        _shop.BuyItem(_originPart, _originItem, _playerInventory);
    }
}
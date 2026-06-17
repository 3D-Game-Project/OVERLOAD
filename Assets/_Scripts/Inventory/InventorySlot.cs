using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image itemIcon;
    public Button slotButton;

    public InventoryPartItem currentPartItem;
    private PartsData currentPart;
    private ItemData currentItem;
    private PlayerInventory inventory;

    [Header("씬 탐색 버그를 막기 위한 다이렉트 캐시 포인터")]
    private MenuController _menu;
    private Shop _shop;
    private SellPopup _sellPopup;
    private PartDetailPopup _detailPopup;
    private SlotOptionPopupUI _slotOptionPopup;

    [Header("Slot Highlight")]
    [SerializeField] private Image _slotBackground;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _equippedColor = new Color(0.35f, 0.75f, 1f, 1f);

    [Header("Durability UI")]
    [SerializeField] private GameObject _durabilityRoot;
    [SerializeField] private Image _durabilityCircle;

    private CorePartsController _corePartsController;
    private PartHoverInfoPopupUI _hoverInfoPopup;


    private void Awake()
    {
        if (slotButton == null) slotButton = GetComponent<Button>();

        if (inventory == null)
            inventory = FindFirstObjectByType<PlayerInventory>();

        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);

        if (_slotBackground == null)
            _slotBackground = GetComponent<Image>();

        if (_corePartsController == null)
            _corePartsController = FindFirstObjectByType<CorePartsController>();

        if (_durabilityRoot == null && _durabilityCircle != null)
            _durabilityRoot = _durabilityCircle.gameObject;
    }

    private void OnEnable()
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnSlotClicked);
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    public void SetMasterReferences(
        MenuController menu,
        Shop shop,
        SellPopup sell,
        PartDetailPopup detail,
        SlotOptionPopupUI slotOptionPopup,
        CorePartsController corePartsController,
        PartHoverInfoPopupUI hoverInfoPopup)
    {
        _menu = menu;
        _shop = shop;
        _sellPopup = sell;
        _detailPopup = detail;
        _slotOptionPopup = slotOptionPopup;
        _corePartsController = corePartsController;
        _hoverInfoPopup = hoverInfoPopup;

        RefreshEquippedHighlight();
    }

    // LEGACY
    //PartsData 정보에 맞춰 슬롯 비주얼 업데이트
    //public void UpdateSlot(PartsData part)
    //{
    //    currentPart = part;
    //    currentPartItem = null;
    //    currentItem = null;

    //    if (part != null)
    //    {
    //        itemIcon.gameObject.SetActive(true);
    //        itemIcon.sprite = part.PartsImage;
    //        itemIcon.color = Color.white ;
    //    }
    //    else
    //    {
    //        SetEmptyVisual();

    //    }

    //    RefreshEquippedHighlight();
    //}

    // 수정한 거에 맞게 추가
    public void UpdateSlot(InventoryPartItem partItem)
    {
        currentPartItem = partItem;
        currentPart = partItem != null ? partItem.PartsData : null;
        currentItem = null;

        if (currentPart == null)
        {
            SetEmptyVisual();
            return;
        }

        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(true);
            itemIcon.sprite = currentPart.PartsImage;
            itemIcon.enabled = currentPart.PartsImage != null;
            itemIcon.color = Color.white;
            itemIcon.preserveAspect = true;
        }

        RefreshEquippedHighlight();
        RefreshDurabilityUI();
    }

    public void UpdateSlot(ItemData item)
    {
        currentPartItem = null;
        currentItem = item;
        currentPart = null;

        if (item != null)
        {
            itemIcon.gameObject.SetActive(true);
            itemIcon.sprite = item.ItemImage;
            itemIcon.color = Color.white;
        }
        else
        {
            SetEmptyVisual();
        }

        RefreshEquippedHighlight();
        RefreshDurabilityUI();
    }

    private void SetEmptyVisual()
    {
        currentPartItem = null;
        currentPart = null;
        currentItem = null;

        if (itemIcon != null)
        {
            itemIcon.sprite = null;

            itemIcon.gameObject.SetActive(false);
        }

        if (_slotBackground != null)
            _slotBackground.color = _normalColor;

        if (_durabilityRoot != null)
            _durabilityRoot.SetActive(false);
    }

    public void OnSlotClicked()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (inventory == null) return;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (_menu == null) _menu = FindFirstObjectByType<MenuController>(FindObjectsInactive.Include);

        bool isCurrentShopTab = (_menu.CurrentTabIndex == 1);
        bool isCurrentInventoryTab = (_menu.CurrentTabIndex == 2);

        if (isCurrentShopTab && _shop != null)
        {
            if (currentPart != null)
            {
                SellPopup[] sellPopups = FindObjectsByType<SellPopup>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                if (sellPopups.Length > 0)
                {
                    foreach (var popup in sellPopups)
                    {
                        popup.OpenPopup(currentPart, _shop, inventory);
                    }
                }
                else
                {
                    _shop.SellPart(currentPart, inventory);
                }
            }
            else if (currentItem != null)
            {
                return;
            }
        }
        else if (isCurrentInventoryTab)
        {
            if (currentPartItem != null && currentPartItem.PartsData != null)
            {
                if (_slotOptionPopup == null)
                    _slotOptionPopup = FindFirstObjectByType<SlotOptionPopupUI>(FindObjectsInactive.Include);

                if (_slotOptionPopup != null)
                {
                    if (_hoverInfoPopup != null)
                    {
                        _hoverInfoPopup.ShowTemporary(currentPartItem);
                    }

                    _slotOptionPopup.Open(this, currentPartItem, inventory);
                }
                else
                {
                    Debug.LogWarning("SlotOptionPopupUI를 찾을 수 없습니다.");
                }
            }
            else if (currentItem != null)
            {
                // 추후 소비 아이템 사용 함수 구역
            }
        }
    }

    private void RefreshEquippedHighlight()
    {
        if (_slotBackground == null)
            return;

        if (currentPartItem == null || _corePartsController == null)
        {
            _slotBackground.color = _normalColor;
            return;
        }

        bool isEquipped = _corePartsController.IsEquipped(currentPartItem);

        _slotBackground.color = isEquipped ? _equippedColor : _normalColor;
    }

    private void RefreshDurabilityUI()
    {
        if (_durabilityRoot == null || _durabilityCircle == null)
            return;

        if (currentPartItem == null || currentPartItem.PartsData == null)
        {
            _durabilityRoot.SetActive(false);
            return;
        }

        float maxDurability = currentPartItem.MaxDurability;

        if (maxDurability <= 0f)
        {
            _durabilityRoot.SetActive(false);
            return;
        }

        float ratio = currentPartItem.CurrentDurability / maxDurability;
        ratio = Mathf.Clamp01(ratio);

        _durabilityRoot.SetActive(true);
        _durabilityCircle.fillAmount = ratio;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentPart == null)
            return;

        if (_hoverInfoPopup == null)
            return;

        _hoverInfoPopup.ShowTemporary(currentPartItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_hoverInfoPopup == null)
            return;

        _hoverInfoPopup.HideIfNotPinned();
    }
}
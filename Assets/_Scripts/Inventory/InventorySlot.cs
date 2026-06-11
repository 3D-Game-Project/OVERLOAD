using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public Button slotButton;

    private PartsData currentPart;
    private ItemData currentItem;
    private PlayerInventory inventory;

    [Header("씬 탐색 버그를 막기 위한 다이렉트 캐시 포인터")]
    private MenuController _menu;
    private Shop _shop;
    private SellPopup _sellPopup;
    private PartDetailPopup _detailPopup;
    private SlotOptionPopupUI _slotOptionPopup;


    private void Awake()
    {
        if (slotButton == null) slotButton = GetComponent<Button>();

        if (inventory == null)
            inventory = FindFirstObjectByType<PlayerInventory>();

        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);
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
        SlotOptionPopupUI slotOptionPopup)
    {
        _menu = menu;
        _shop = shop;
        _sellPopup = sell;
        _detailPopup = detail;
        _slotOptionPopup = slotOptionPopup;
    }

    //PartsData 정보에 맞춰 슬롯 비주얼 업데이트
    public void UpdateSlot(PartsData part)
    {
        currentPart = part;
        currentItem = null;

        if (part != null)
        {
            itemIcon.gameObject.SetActive(true);
            itemIcon.sprite = part.PartsImage;
            itemIcon.color = Color.white ;
        }
        else
        {
            SetEmptyVisual();

        }
    }

    public void UpdateSlot(ItemData item)
    {
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
    }

    private void SetEmptyVisual()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = null;

            itemIcon.gameObject.SetActive(false);
        }
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
            if (currentPart != null)
            {
                if (_slotOptionPopup == null)
                    _slotOptionPopup = FindFirstObjectByType<SlotOptionPopupUI>(FindObjectsInactive.Include);

                if (_slotOptionPopup != null)
                {
                    _slotOptionPopup.Open(this, currentPart, inventory);
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
}
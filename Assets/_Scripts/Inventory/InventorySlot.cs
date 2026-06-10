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


    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<PlayerInventory>();

        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);
    }

    public void SetMasterReferences(MenuController menu, Shop shop, SellPopup sell, PartDetailPopup detail)
    {
        _menu = menu;
        _shop = shop;
        _sellPopup = sell;
        _detailPopup = detail;
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
            Debug.Log("판매처리");

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
        if (inventory == null) return;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }


        bool isCurrentShopTab = (_menu.CurrentTabIndex == 1);
        bool isCurrentInventoryTab = (_menu.CurrentTabIndex == 2);

        Debug.Log($"[OnSlotClicked] 클릭 집행 ➔ 현재 UI 활성 탭 인덱스: {_menu.CurrentTabIndex} (Shop패널여부: {isCurrentShopTab} / Inv패널여부: {isCurrentInventoryTab})");

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
                    // 백업용 즉시 판매
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
                PartDetailPopup[] detailPopups = FindObjectsByType<PartDetailPopup>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                foreach (var popup in detailPopups)
                {
                    popup.OpenPopup(currentPart, inventory);
                }
            }
            else if (currentItem != null)
            {
                // 추후 아이템 사용 함수 구역
            }
        }
    }
}
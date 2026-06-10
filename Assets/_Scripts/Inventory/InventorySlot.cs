using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public Button slotButton;

    private PartsData currentPart;
    private ItemData currentItem;
    private PlayerInventory inventory;


    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<PlayerInventory>();

        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);
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

        MenuController menu = FindFirstObjectByType<MenuController>();
        Shop shop = FindFirstObjectByType<Shop>();

        if (menu == null) return;

        bool isCurrentShopTab = (menu.CurrentTabIndex == 1);
        bool isCurrentInventoryTab = (menu.CurrentTabIndex == 2);

        Debug.Log($"[OnSlotClicked] 클릭 집행 ➔ 현재 UI 활성 탭 인덱스: {menu.CurrentTabIndex} (Shop패널여부: {isCurrentShopTab} / Inv패널여부: {isCurrentInventoryTab})");

        if (isCurrentShopTab && shop != null)
        {
            if (currentPart != null)
            {
                SellPopup[] sellPopups = FindObjectsByType<SellPopup>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                if (sellPopups.Length > 0)
                {
                    foreach (var popup in sellPopups)
                    {
                        popup.OpenPopup(currentPart, shop, inventory);
                    }
                }
                else
                {
                    // 백업용 즉시 판매
                    shop.SellPart(currentPart, inventory);
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
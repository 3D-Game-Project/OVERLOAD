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

        if (menu != null && menu.IsShop && shop != null)
        {
            if (currentPart != null)
            {
                SellPopup sellPopup = FindFirstObjectByType<SellPopup>(FindObjectsInactive.Include);

                if (sellPopup != null)
                {
                    sellPopup.OpenPopup(currentPart, shop, inventory);
                }
                else
                {
                    shop.SellPart(currentPart, inventory);
                }
            }
            else if (currentItem != null)
            {
                return;
            }
        }
        else
        {
            if (currentPart != null)
            {
                //inventory.EquipParts(currentPart);
            }
            else if (currentItem != null)
            {
                // 추후 아이템 사용 함수
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public Button slotButton;

    private PartsData currentPart;
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

        if (part != null)
        {
            itemIcon.gameObject.SetActive(true);

            itemIcon.color = Color.black ;
        }
        else
        {
            Debug.Log("UpdateSlot 호출됨 - 아이템 없음");
            itemIcon.gameObject.SetActive(true);

            itemIcon.sprite = null;
            itemIcon.color = new Color(166f / 255f, 166f / 255f, 166f / 255f, 1f);
            
        }
    }

    public void OnSlotClicked()
    {
        if (inventory == null || currentPart == null) return;

    }
}
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotParent;

    private List<InventorySlot> uiSlots = new List<InventorySlot>();

    private void Awake()
    {
        if (inventory == null)
            inventory = GetComponentInParent<PlayerInventory>();
    }

    // 시작시 인벤토리 데이터와 UI 슬롯을 연결하여 초기화
    private void Start()
    {
        if (inventory != null)
        {
            GenerateSlots();
            RefreshUI();
        }
    }

    private void OnEnable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += RefreshUI;
    }
    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshUI;
    }

    // 5x5 Grid 형태 슬롯 생성
    private void GenerateSlots()
    {
        for (int i = 0; i < inventory.InventorySize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            uiSlots.Add(slot);
        }
    }

    // 파츠 드랍 및 장착에 따른 인벤토리 슬롯 변경
    private void RefreshUI()
    {

        if (inventory == null)
        {
            return;
        }

        if (inventory.PartsList == null)
        {
            return;
        }

        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (uiSlots[i] == null) continue;

            if (i < inventory.PartsList.Count)
                uiSlots[i].UpdateSlot(inventory.PartsList[i]);
            else
            {
                uiSlots[i].UpdateSlot(null);
            }
        }
    }
}
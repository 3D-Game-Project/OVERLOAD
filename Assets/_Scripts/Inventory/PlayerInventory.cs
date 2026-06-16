using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int InventorySize = 20;
    public int ConsumableSize = 5;

    [Header("Parts Inventory")]
    public List<InventoryPartItem> PartItems = new List<InventoryPartItem>();
    public List<ItemData> ConsumablesList = new List<ItemData>();

    // 임시용
    public List<PartsData> PartsList
    {
        get
        {
            List<PartsData> result = new List<PartsData>();

            foreach (InventoryPartItem item in PartItems)
            {
                if (item != null && item.PartsData != null)
                    result.Add(item.PartsData);
            }

            return result;
        }
    }

    public Dictionary<string, int> CurrencyList = new Dictionary<string, int>();


    public event Action OnInventoryChanged; 

    private PlayerInputHandler _inputHandler;

    // 시작시 PlayerInputHandler 컴포넌트 참조
    // 재화 초기 설정 수정
    private void Awake()
    {
        if (!CurrencyList.ContainsKey("Gear")) CurrencyList.Add("Gear", 100);
        if (!CurrencyList.ContainsKey("Scrap")) CurrencyList.Add("Scrap", 50);
    }

    // 파츠 추가 함수 - PlayerInteractionController의 Pickup시 호출
    // 아이템 유효성 검증 후 인벤토리에 추가 및 UI 갱신
    //public void AddPart(PartsData item)
    //{
    //    if (item == null) return;

    //    if (PartItems.Count < InventorySize)
    //    {
    //        InventoryPartItem newPartItem = new InventoryPartItem(item);
    //        PartItems.Add(newPartItem);

    //        Debug.Log($"[Inventory] {item.name} 획득 및 추가 완료. ID: {newPartItem.InstanceId}");
    //        OnInventoryChanged?.Invoke();
    //    }
    //    else
    //    {
    //        Debug.LogWarning("인벤토리가 가득 찼습니다!");
    //    }
    //}

    public InventoryPartItem AddPart(PartsData item)
    {
        if (item == null)
            return null;

        if (PartItems.Count < InventorySize)
        {
            InventoryPartItem newPartItem = new InventoryPartItem(item);
            PartItems.Add(newPartItem);

            Debug.Log($"[Inventory] {item.name} 획득 및 추가 완료. ID: {newPartItem.InstanceId}");

            OnInventoryChanged?.Invoke();

            return newPartItem;
        }
        else
        {
            Debug.LogWarning("인벤토리가 가득 찼습니다!");
            return null;
        }
    }

    public void AddConsumable(ItemData item)
    {
        if (item == null) return;

        if (ConsumablesList.Count < ConsumableSize)
        {
            ConsumablesList.Add(item);
            Debug.Log($"[Inventory] {item.name} 획득 및 추가 완료.");
            OnInventoryChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning("인벤토리가 가득 찼습니다!");
        }
    }

    // 파츠 장착 함수 - 인벤토리 UI에서 장착 버튼 클릭 시 호출
    // 파츠 장착 로직을 구현할 때 추가 구현할 예정
    public void EquipParts(PartsData item)
    {
        // 선택한 Slot에 존재하는 파츠를 파라미터로 받아서 장착처리
        // 장착에 대한 검증 함수 호출 (Bool타입 함수)
        // True일 경우 아이템 제거
        // 이후 UI갱신을 위한 이벤트 발행
    }

    public bool RemovePartItem(InventoryPartItem item)
    {
        if (item == null)
            return false;

        bool removed = PartItems.Remove(item);

        if (removed)
        {
            Debug.Log($"[Inventory] 파츠 제거 완료: {item.PartsData?.PartsName} / ID: {item.InstanceId}");
            OnInventoryChanged?.Invoke();
        }

        return removed;
    }

    // 판매시 파츠 인벤토리에서 제거하는 함수
    public bool RemovePart(PartsData item)
    {
        if (item == null)
            return false;

        InventoryPartItem targetItem = null;

        foreach (InventoryPartItem partItem in PartItems)
        {
            if (partItem != null && partItem.PartsData == item)
            {
                targetItem = partItem;
                break;
            }
        }

        if (targetItem == null)
            return false;

        return RemovePartItem(targetItem);
    }

    public void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}
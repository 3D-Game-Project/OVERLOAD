using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int InventorySize = 25; 

    public List<PartsData> PartsList = new List<PartsData>();

    public Dictionary<string, int> CurrencyList = new Dictionary<string, int>();


    public event Action OnInventoryChanged; 

    private PlayerInputHandler _inputHandler;

    // 시작시 PlayerInputHandler 컴포넌트 참조
    // 재화 초기 설정 수정
    private void Awake()
    {
        CurrencyList.Add("Gold", 0);
        CurrencyList.Add("Scrap", 0);
    }

    // 파츠 추가 함수 - PlayerInteractionController의 Pickup시 호출
    // 아이템 유효성 검증 후 인벤토리에 추가 및 UI 갱신
    public void AddItem(PartsData item)
    {
        if (item == null) return;

        if (ValidateItem(item))
        {
            if (PartsList.Count < InventorySize)
            {
                PartsList.Add(item);
                Debug.Log($"[Inventory] {item.name} 획득 및 추가 완료.");
                OnInventoryChanged?.Invoke(); 
            }
            else
            {
                Debug.LogWarning("인벤토리가 가득 찼습니다!");
            }
        }
    }

    // 아이템 유효성 검증 함수
    // 파츠에 자식 SO(AttackParts, ArmorParts, UtilityParts 등)가 존재하는지 확인
    private bool ValidateItem(PartsData item)
    {
        //if (item == null)
        //{
        //    Debug.LogWarning("잘못된 아이템 데이터입니다.");
        //    return false;
        //}

        return true;
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
}
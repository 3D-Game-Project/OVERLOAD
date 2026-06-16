using System;
using UnityEngine;

[Serializable]
public class InventoryPartItem
{
    [SerializeField] private string _instanceId;
    [SerializeField] private PartsData _partsData;
    [SerializeField] private float _currentDurability;
    [SerializeField] private bool _isEquipped;

    public string InstanceId => _instanceId;
    public PartsData PartsData => _partsData;
    public float CurrentDurability => _currentDurability;
    public bool IsEquipped => _isEquipped;

    public float MaxDurability
    {
        get
        {
            if (_partsData == null)
                return 0f;

            return _partsData.MaxDurability;
        }
    }

    public bool IsDestroyed => _currentDurability <= 0f;

    // 생성자
    // 인벤토리에 사용할때 기본값 생성
    public InventoryPartItem(PartsData partsData)
    {
        _instanceId = Guid.NewGuid().ToString();
        _partsData = partsData;

        if (_partsData != null)
            _currentDurability = _partsData.MaxDurability;
        else
            _currentDurability = 0f;

        _isEquipped = false;
    }

    // 파츠 장착/해제 후 해당 파츠의 장착 상태 기록용
    public void SetEquipped(bool equipped)
    {
        _isEquipped = equipped;
    }

    // 내구도 초기화용 (내구도 감소 및 수리로 증가)
    public void SetDurability(float durability)
    {
        _currentDurability = Mathf.Clamp(durability, 0f, MaxDurability);
    }

    public void ReduceDurability(float amount)
    {
        if (amount <= 0f)
            return;

        SetDurability(_currentDurability - amount);
    }

    // 최대로 수리
    public void RepairToFull()
    {
        SetDurability(MaxDurability);
    }

    // 내구도 수리 판단용
    public bool CanRepair()
    {
        if (_partsData == null)
            return false;

        return _currentDurability < MaxDurability;
    }
}
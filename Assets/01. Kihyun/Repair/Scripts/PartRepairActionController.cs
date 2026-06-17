using System;
using System.Collections;
using UnityEngine;

public class PartRepairActionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory _inventory;
    [SerializeField] private CorePartsController _corePartsController;
    [SerializeField] private PartEquipActionController _partEquipActionController;

    [Header("Repair Time")]
    [SerializeField] private float _repairTimePer100Durability = 2f;
    [SerializeField] private float _minimumRepairTime = 0.25f;

    private Coroutine _repairCoroutine;

    private InventoryPartItem _currentPartItem;
    private RepairKitData _currentRepairKit;
    private float _currentRepairAmount;
    private float _requiredTime;
    private float _elapsedTime;

    public bool IsBusy => _repairCoroutine != null;

    public InventoryPartItem CurrentPartItem => _currentPartItem;
    public float Progress
    {
        get
        {
            if (_requiredTime <= 0f)
                return 0f;

            return Mathf.Clamp01(_elapsedTime / _requiredTime);
        }
    }

    public event Action<InventoryPartItem, float> OnRepairProgress;
    public event Action<bool> OnRepairFinished;

    private void Awake()
    {
        if (_inventory == null)
            _inventory = GetComponent<PlayerInventory>();

        if (_corePartsController == null)
            _corePartsController = GetComponent<CorePartsController>();

        if (_partEquipActionController == null)
            _partEquipActionController = GetComponent<PartEquipActionController>();
    }

    public bool TryStartRepair(InventoryPartItem partItem)
    {
        if (IsBusy)
        {
            Debug.LogWarning("[PartRepairAction] 이미 수리 중입니다.");
            return false;
        }

        if (_partEquipActionController != null && _partEquipActionController.IsBusy)
        {
            Debug.LogWarning("[PartRepairAction] 장착/해제 작업 중에는 수리할 수 없습니다.");
            return false;
        }

        if (_inventory == null)
        {
            Debug.LogWarning("[PartRepairAction] PlayerInventory가 없습니다.");
            return false;
        }

        if (partItem == null || partItem.PartsData == null)
        {
            Debug.LogWarning("[PartRepairAction] 수리할 파츠가 없습니다.");
            return false;
        }

        if (partItem.IsFullDurability)
        {
            Debug.LogWarning($"[PartRepairAction] 이미 최대 내구도입니다: {partItem.PartsData.PartsName}");
            return false;
        }

        if (!_inventory.HasRepairKit)
        {
            Debug.LogWarning("[PartRepairAction] RepairKit이 없습니다.");
            return false;
        }

        RepairKitData repairKit = _inventory.RepairKitData;

        if (repairKit == null)
        {
            Debug.LogWarning("[PartRepairAction] RepairKitData가 없습니다.");
            return false;
        }

        float actualRepairAmount = Mathf.Min(
            repairKit.RepairAmount,
            partItem.MissingDurability
        );

        if (actualRepairAmount <= 0f)
        {
            Debug.LogWarning("[PartRepairAction] 실제 수리량이 없습니다.");
            return false;
        }

        float requiredTime =
            actualRepairAmount / 100f * _repairTimePer100Durability;

        requiredTime = Mathf.Max(requiredTime, _minimumRepairTime);

        bool consumed = _inventory.ConsumeRepairKit();

        if (!consumed)
        {
            Debug.LogWarning("[PartRepairAction] RepairKit 소모 실패.");
            return false;
        }

        _currentPartItem = partItem;
        _currentRepairKit = repairKit;
        _currentRepairAmount = actualRepairAmount;
        _requiredTime = requiredTime;
        _elapsedTime = 0f;

        _repairCoroutine = StartCoroutine(RepairRoutine());

        Debug.Log(
            $"[PartRepairAction] 수리 시작: {partItem.PartsData.PartsName} / " +
            $"수리량 {_currentRepairAmount:0} / 시간 {_requiredTime:0.00}s"
        );

        return true;
    }

    private IEnumerator RepairRoutine()
    {
        while (_elapsedTime < _requiredTime)
        {
            _elapsedTime += Time.deltaTime;

            OnRepairProgress?.Invoke(_currentPartItem, Progress);

            yield return null;
        }

        CompleteRepair();
    }

    private void CompleteRepair()
    {
        bool success = false;

        if (_currentPartItem != null)
        {
            float repairedAmount = _currentPartItem.RepairBy(_currentRepairAmount);

            SyncEquippedDurability(_currentPartItem);

            Debug.Log(
                $"[PartRepairAction] 수리 완료: {_currentPartItem.PartsData.PartsName} / " +
                $"실제 회복량 {repairedAmount:0} / " +
                $"{_currentPartItem.CurrentDurability:0} / {_currentPartItem.MaxDurability:0}"
            );

            _inventory?.NotifyInventoryChanged();

            success = true;
        }

        ClearCurrentRepair();

        OnRepairFinished?.Invoke(success);
    }

    private void SyncEquippedDurability(InventoryPartItem partItem)
    {
        if (_corePartsController == null)
            return;

        AttachmentSlot equippedSlot = _corePartsController.FindEquippedSlot(partItem);

        if (equippedSlot == null || equippedSlot.AttachedObject == null)
            return;

        DurabilityController[] durabilities =
            equippedSlot.AttachedObject.GetComponentsInChildren<DurabilityController>(true);

        foreach (DurabilityController durability in durabilities)
        {
            durability.InitializeFromInventoryItem(partItem);
        }
    }

    private void ClearCurrentRepair()
    {
        _repairCoroutine = null;

        _currentPartItem = null;
        _currentRepairKit = null;
        _currentRepairAmount = 0f;
        _requiredTime = 0f;
        _elapsedTime = 0f;
    }
}
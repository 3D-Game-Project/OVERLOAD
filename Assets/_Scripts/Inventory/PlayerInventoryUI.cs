using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerInventory _inventory;
    [SerializeField] private GameObject _slotPrefab;

    [SerializeField] private Transform _partsSlotParent;
    [SerializeField] private Transform _consumablesSlotParent;

    private List<InventorySlot> _partsUISlots = new List<InventorySlot>();
    private List<InventorySlot> _consumablesUISlots = new List<InventorySlot>();

    private void Awake()
    {
        _inventory = FindFirstObjectByType<PlayerInventory>();
    }

    // 시작시 인벤토리 데이터와 UI 슬롯을 연결하여 초기화
    private void Start()
    {
        if (_inventory != null)
        {
            GenerateSlots();
            RefreshUI();
        }
    }

    private void OnEnable()
    {
        if (_inventory == null)
            _inventory = FindFirstObjectByType<PlayerInventory>();

        if (_inventory != null)
        {
            _inventory.OnInventoryChanged += RefreshUI;

            if (_partsUISlots.Count == 0 || _consumablesUISlots.Count == 0)
            {
                GenerateSlots();
            }

            RefreshUI();
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerInventory 데이터를 씬에서 찾을 수 없어 슬롯 생성을 스킵합니다!");
        }
    }

    private void OnDisable()
    {
        if (_inventory != null)
        {
            _inventory.OnInventoryChanged -= RefreshUI;
        }
    }

    // 5x4 Grid 형태 슬롯 생성
    private void GenerateSlots()
    {
        if (_inventory == null) return;

        foreach (Transform child in _partsSlotParent)
        {
            Destroy(child.gameObject);
        }
        _partsUISlots.Clear();

        for (int i = 0; i < _inventory.InventorySize; i++)
        {
            GameObject slotObj = Instantiate(_slotPrefab, _partsSlotParent);
            _partsUISlots.Add(slotObj.GetComponent<InventorySlot>());
        }

        foreach (Transform child in _consumablesSlotParent) 
        {
            Destroy(child.gameObject);
        }
        
        _consumablesUISlots.Clear();

        for (int i = 0; i < _inventory.ConsumableSize; i++)
        {
            GameObject slotObj = Instantiate(_slotPrefab, _consumablesSlotParent);
            _consumablesUISlots.Add(slotObj.GetComponent<InventorySlot>());
        }
    }

    // 파츠 드랍 및 장착에 따른 인벤토리 슬롯 변경
    public void RefreshUI()
    {
        if (_inventory == null) return;

        for (int i = 0; i < _partsUISlots.Count; i++)
        {
            if (_partsUISlots[i] == null) continue;

            if (i < _inventory.PartsList.Count)
                _partsUISlots[i].UpdateSlot(_inventory.PartsList[i]);
            else
                _partsUISlots[i].UpdateSlot((PartsData)null);
        }

        for (int i = 0; i < _consumablesUISlots.Count; i++)
        {
            if (_consumablesUISlots[i] == null) continue;

            if (i < _inventory.ConsumablesList.Count)
                _consumablesUISlots[i].UpdateSlot(_inventory.ConsumablesList[i]);
            else
                _consumablesUISlots[i].UpdateSlot((ItemData)null);
        }
    }
}
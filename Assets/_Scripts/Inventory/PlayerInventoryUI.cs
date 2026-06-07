using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerInventory _inventory;
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _slotParent;

    private List<InventorySlot> _uiSlots = new List<InventorySlot>();

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

            if (_uiSlots.Count == 0)
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

    // 5x5 Grid 형태 슬롯 생성
    private void GenerateSlots()
    {
        if (_inventory == null) return;

        foreach (Transform child in _slotParent)
        {
            Destroy(child.gameObject);
        }
        _uiSlots.Clear();

        for (int i = 0; i < _inventory.InventorySize; i++)
        {
            GameObject slotObj = Instantiate(_slotPrefab, _slotParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            _uiSlots.Add(slot);
        }
    }

    // 파츠 드랍 및 장착에 따른 인벤토리 슬롯 변경
    private void RefreshUI()
    {
        if (_inventory == null || _inventory.PartsList == null) return;

        for (int i = 0; i < _uiSlots.Count; i++)
        {
            if (_uiSlots[i] == null) continue;

            if (i < _inventory.PartsList.Count)
                _uiSlots[i].UpdateSlot(_inventory.PartsList[i]);
            else
            {
                _uiSlots[i].UpdateSlot(null);
            }
        }
    }
}
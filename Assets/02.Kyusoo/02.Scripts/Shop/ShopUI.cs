using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private Shop _shopSystem;
    [SerializeField] private PlayerInventory _playerInventory;

    [Header("UI 슬롯 위치r")]
    [SerializeField] private GameObject _shopSlotPrefab; 
    [SerializeField] private Transform _shopList;

    private void Awake()
    {
        // 캐싱 연산을 Awake로 이동하여 안전하게 사전 확보
        if (_shopSystem == null) _shopSystem = FindFirstObjectByType<Shop>();
        if (_playerInventory == null) _playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    private void Start()
    {
        RefreshShopListUI();
    }

    private void OnEnable()
    {
        if (_shopSystem == null) _shopSystem = FindFirstObjectByType<Shop>();

        if (_shopSystem != null)
        {
            _shopSystem.ShopListUpdated -= RefreshShopListUI;
            _shopSystem.ShopListUpdated += RefreshShopListUI;
        }

        RefreshShopListUI();
    }

    private void OnDisable()
    {
        if (_shopSystem != null)
        {
            _shopSystem.ShopListUpdated -= RefreshShopListUI;
        }
    }

    // 파츠, 아이템 일괄 리스트업
    // 파츠가 우선적으로 리스트업 되도록 처리
    // 이후, 아이템이 리스트업되도록 처리
    private void RefreshShopListUI()
    {
        if (_shopSystem == null) return;
        if (_shopList == null) return;
        if(_shopSlotPrefab == null) return;

        foreach (Transform child in _shopList)
        {
            Destroy(child.gameObject);
        }

        if(_shopSystem.ExistParts.Count > 0)
        {
            foreach (PartsData part in _shopSystem.ExistParts)
            {
                GameObject slotObj = Instantiate(_shopSlotPrefab, _shopList);
                if (slotObj.TryGetComponent(out ShopSlot slot))
                {
                    slot.SetupSlot(part, _shopSystem, _playerInventory);
                }
            }
        }
        foreach (ItemData item in _shopSystem.ExistItems)
        {
            GameObject slotObj = Instantiate(_shopSlotPrefab, _shopList);
            if (slotObj.TryGetComponent(out ShopSlot slot))
            {
                slot.SetupSlot(item, _shopSystem, _playerInventory);
            }
        }
    }
}
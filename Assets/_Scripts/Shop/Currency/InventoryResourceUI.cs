using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventoryResourceUI : MonoBehaviour
{
    [Header("플레이어 인벤토리 참조")]
    [SerializeField] private PlayerInventory _inventory;

    [Header("재화 표시용 텍스트 (TMP)")]
    [SerializeField] private TextMeshProUGUI _scrapText;
    [SerializeField] private TextMeshProUGUI _gearText;

    [Header("Repair Kit 표시")]
    [SerializeField] private Image _repairKitIcon;
    [SerializeField] private TextMeshProUGUI _repairKitText;

    [Header("Option")]
    [SerializeField] private bool _hideRepairKitWhenZero = false;
    [SerializeField] private GameObject _repairKitRoot;


    private void Awake()
    {
        if (_inventory == null)
            _inventory = FindFirstObjectByType<PlayerInventory>();

        if (_repairKitRoot == null && _repairKitText != null)
            _repairKitRoot = _repairKitText.transform.parent.gameObject;
    }

    private void OnEnable()
    {
        if (_inventory == null)
            _inventory = FindFirstObjectByType<PlayerInventory>();

        if (_inventory != null)
        {
            _inventory.OnInventoryChanged -= RefreshResourceUI;
            _inventory.OnInventoryChanged += RefreshResourceUI;

            RefreshResourceUI();
        }
    }

    private void OnDisable()
    {
        if (_inventory != null)
        {
            _inventory.OnInventoryChanged -= RefreshResourceUI;
        }
    }

    // RefreshCurreny로 이동후 RefreshResourceUI에서 통합
    //public void RefreshCurrencyUI()
    //{
    //    if (_inventory == null) return;

    //    if (_scrapText != null && _inventory.CurrencyList.ContainsKey("Scrap"))
    //    {
    //        int scrap = _inventory.CurrencyList["Scrap"];
    //        _scrapText.text = scrap.ToString("N0"); 
    //    }

    //    if (_gearText != null && _inventory.CurrencyList.ContainsKey("Gear"))
    //    {
    //        int gear = _inventory.CurrencyList["Gear"];
    //        _gearText.text = gear.ToString("N0");
    //    }
    //}

    public void RefreshResourceUI()
    {
        if (_inventory == null)
            return;

        RefreshCurrency();
        RefreshRepairKit();
    }

    private void RefreshCurrency()
    {
        if (_scrapText != null && _inventory.CurrencyList.ContainsKey("Scrap"))
        {
            int scrap = _inventory.CurrencyList["Scrap"];
            _scrapText.text = scrap.ToString("N0");
        }

        if (_gearText != null && _inventory.CurrencyList.ContainsKey("Gear"))
        {
            int gear = _inventory.CurrencyList["Gear"];
            _gearText.text = gear.ToString("N0");
        }
    }

    private void RefreshRepairKit()
    {
        int repairKitCount = _inventory.RepairKitCount;

        if (_repairKitText != null)
            _repairKitText.text = repairKitCount.ToString("N0");

        if (_repairKitIcon != null && _inventory.RepairKitData != null)
        {
            _repairKitIcon.sprite = _inventory.RepairKitData.ItemImage;
            _repairKitIcon.enabled = _inventory.RepairKitData.ItemImage != null;
        }

        if (_repairKitRoot != null)
        {
            bool shouldShow = !_hideRepairKitWhenZero || repairKitCount > 0;
            _repairKitRoot.SetActive(shouldShow);
        }
    }
}
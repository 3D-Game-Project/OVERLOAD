using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    [Header("플레이어 인벤토리 참조")]
    [SerializeField] private PlayerInventory _inventory;

    [Header("재화 표시용 텍스트 (TMP)")]
    [SerializeField] private TextMeshProUGUI _scrapText;
    [SerializeField] private TextMeshProUGUI _gearText;

    private void Awake()
    {
        if (_inventory == null)
            _inventory = FindFirstObjectByType<PlayerInventory>();
    }

    private void OnEnable()
    {
        if (_inventory != null)
        {
            _inventory.OnInventoryChanged -= RefreshCurrencyUI;
            _inventory.OnInventoryChanged += RefreshCurrencyUI;

            RefreshCurrencyUI();
        }
    }

    private void OnDisable()
    {
        if (_inventory != null)
        {
            _inventory.OnInventoryChanged -= RefreshCurrencyUI;
        }
    }

    public void RefreshCurrencyUI()
    {
        if (_inventory == null) return;

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
}
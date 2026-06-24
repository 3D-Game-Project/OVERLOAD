using UnityEngine;
using System.Collections.Generic;
using System;

public class Shop : MonoBehaviour
{
    [SerializeField] private List<PartsData> _existParts = new List<PartsData>();
    [SerializeField] private List<ItemData> _existItems = new List<ItemData>();
    [SerializeField] private List<PartsData> _shopListParts = new List<PartsData>();

    [Header("Sell")]
    [Range(0f, 1f)] [SerializeField] private float _sellRefundRate = 0.5f;

    public List<ItemData> ExistItems => _existItems;
    public List<PartsData> ExistParts => _shopListParts;

    public event Action ShopListUpdated;
    //public event Action<int> CurrencyUpdated;

    private void Start()
    {
        AddShopList();
    }
    public void CheckExistParts(PartsData part)
    {
        if (part == null) return;

        if (string.IsNullOrEmpty(part.PartsName)) return;

        if (_existParts.Contains(part)) 
        {
            Debug.Log("이미 _existParts에 존재하는 Parts입니다.");
            return;
        }

        _existParts.Add(part);
        AddShopList();

    }

    public void AddShopList()
    {
        _shopListParts.Clear();
        foreach (var part in _existParts)
        {
            _shopListParts.Add(part);
        }
        ShopListUpdated?.Invoke();
    }

    //public void BuyItem(PartsData part, ItemData item, PlayerInventory playerInventory)
    //{
    //    if (playerInventory == null) return;
    //    if (part == null && item == null) return;

    //    int price = 0;
    //    string itemName = "";
    //    string currencyKey = "";

    //    if (part != null)
    //    {
    //        price = part.Price;
    //        itemName = part.PartsName;
    //        currencyKey = "Scrap";
    //    }
    //    else if(item != null)
    //    {
    //        price = item.Price;
    //        itemName = item.ItemName;
    //        currencyKey = "Gear";
    //    }

    //    int currentCurrency = playerInventory.CurrencyList[currencyKey];

    //    if (currentCurrency < price)
    //    {
    //        Debug.LogWarning($"{itemName} 구매 실패: 보유 재화가 부족합니다.");
    //        return;
    //    }

    //    if (!CheckInventorySize(playerInventory, part, item))
    //    {
    //        Debug.Log("인벤토리가 가득 찼습니다.");
    //        return;
    //    }

    //    playerInventory.CurrencyList[currencyKey] -= price;
    //    CurrencyUpdated?.Invoke(playerInventory.CurrencyList[currencyKey]);

    //    if (part != null) playerInventory.AddPart(part);
    //    if (item != null) playerInventory.AddConsumable(item);

    //    RemoveShopItem(part, item);
    //}

    public void BuyItem(PartsData part, ItemData item, PlayerInventory playerInventory)
    {
        if (playerInventory == null)
            return;

        if (part == null && item == null)
            return;

        CurrencyCost buyCost;
        string itemName;

        if (part != null)
        {
            buyCost = part.BuyCost;
            itemName = part.PartsName;
        }
        else
        {
            buyCost = item.BuyCost;
            itemName = item.ItemName;
        }

        if (!CheckInventorySize(playerInventory, part, item))
        {
            Debug.LogWarning(
                $"[Shop] {itemName} 구매 실패: 인벤토리가 가득 찼습니다.");
            return;
        }

        if (!playerInventory.TrySpendCurrency(buyCost))
        {
            Debug.LogWarning(
                $"[Shop] {itemName} 구매 실패\n" +
                $"필요 Gear: {buyCost.Gear}, 보유: {playerInventory.GetCurrency("Gear")}\n" +
                $"필요 Scrap: {buyCost.Scrap}, 보유: {playerInventory.GetCurrency("Scrap")}"
            );
            return;
        }

        if (part != null)
            playerInventory.AddPart(part);
        else
            playerInventory.AddConsumable(item);

        RemoveShopItem(part, item);

        Debug.Log(
            $"[Shop] {itemName} 구매 완료\n" +
            $"소모 Gear: {buyCost.Gear}, Scrap: {buyCost.Scrap}"
        );
    }


    //private bool CheckInventorySize(PlayerInventory playerInventory, PartsData part, ItemData item)
    //{
    //    if (playerInventory == null) return false;

    //    if (part != null) return playerInventory.PartsList.Count < 20;
    //    if (item != null) return playerInventory.ConsumablesList.Count < 5;


    //    return true;
    //}

    private bool CheckInventorySize(PlayerInventory playerInventory, PartsData part, ItemData item)
    {
        if (playerInventory == null)
            return false;

        if (part != null)
        {
            return playerInventory.PartItems.Count <
                   playerInventory.InventorySize;
        }

        // RepairKit은 별도 개수로 중첩되므로 일반 슬롯을 차지하지 않는다.
        if (item is RepairKitData)
            return true;

        if (item != null)
        {
            return playerInventory.ConsumablesList.Count <
                   playerInventory.ConsumableSize;
        }

        return false;
    }

    private void RemoveShopItem(PartsData part, ItemData item)
    {
        if(part == null && item == null) return;

        if(part != null)
        {
            _shopListParts.Remove(part);
        }

        ShopListUpdated?.Invoke();
    }

    //public void SellPart(PartsData part, PlayerInventory playerInventory)
    //{
    //    if (playerInventory == null || part == null) return;

    //    if (playerInventory.RemovePart(part))
    //    {
    //        int sellPrice = part.Price / 2;

    //        playerInventory.CurrencyList["Scrap"] += sellPrice;

    //        CurrencyUpdated?.Invoke(playerInventory.CurrencyList["Scrap"]);
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"인벤토리에 판매할 아이템이 존재하지 않습니다.");
    //    }
    //}

    public CurrencyCost GetSellReward(PartsData part)
    {
        if (part == null)
            return default;

        return new CurrencyCost
        {
            Gear = Mathf.FloorToInt(
                part.BuyCost.Gear * _sellRefundRate),

            Scrap = Mathf.FloorToInt(
                part.BuyCost.Scrap * _sellRefundRate)
        };
    }

    public void SellPart(
        InventoryPartItem partItem,
        PlayerInventory playerInventory)
    {
        if (playerInventory == null ||
            partItem == null ||
            partItem.PartsData == null)
        {
            return;
        }

        PartsData part = partItem.PartsData;
        CurrencyCost reward = GetSellReward(part);

        if (!playerInventory.RemovePartItem(partItem))
        {
            Debug.LogWarning(
                $"[Shop] 판매할 파츠를 인벤토리에서 찾지 못했습니다: " +
                $"{part.PartsName}");
            return;
        }

        playerInventory.AddCurrency(reward);

        Debug.Log(
            $"[Shop] {part.PartsName} 판매 완료\n" +
            $"획득 Gear: {reward.Gear}, Scrap: {reward.Scrap}"
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MenuController menu = FindFirstObjectByType<MenuController>();
            if (menu != null)
            {
                menu.IsShop = true;
                Debug.Log("상점 진입");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어가 상점 영역을 완전히 벗어났을 때
        if (other.CompareTag("Player"))
        {
            MenuController menu = FindFirstObjectByType<MenuController>();
            if (menu != null)
            {
                menu.IsShop = false;
                menu.CloseMenu();
                Debug.Log("상점 이탈");
            }
        }
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "New_Item_Data", menuName = "Data/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string _itemName;
    [SerializeField] private ItemType _itemType;
    [TextArea(1, 3)][SerializeField] private string _description;
    [SerializeField] private Sprite _itemImage;
    [SerializeField] private CurrencyCost _buyCost;

    public string ItemName => _itemName;
    public ItemType ItemType => _itemType;
    public string Description => _description;
    public Sprite ItemImage => _itemImage;
    public CurrencyCost BuyCost => _buyCost;
}

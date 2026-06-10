using UnityEngine;

[CreateAssetMenu(fileName = "New_Drop_Item", menuName = "Data/Drop Item")]
public class DropItem : ScriptableObject
{
    [SerializeField] private CurrencyType _currencyType; 
    [Range(0f, 100f)][SerializeField] private float _dropChance = 100f;

    [Header("비주얼 및 실물 프리팹")]
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private Sprite _itemImage;

    public CurrencyType CurrencyType => _currencyType;
    public float DropChance => _dropChance;
    public GameObject ItemPrefab => _itemPrefab;
    public Sprite ItemImage => _itemImage;
    
}

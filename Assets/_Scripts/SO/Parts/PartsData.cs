using UnityEngine;

public abstract class PartsData : ScriptableObject
{
    [SerializeField] private string _partsName;
    [SerializeField] private GameObject _partsPrefab;
    [SerializeField] private int _requiredLoad;
    [SerializeField] private Sprite _partsImage;
    [SerializeField] private float _maxDurability;
    [SerializeField] private int _price;
    [TextArea(1,3)][SerializeField] private string _description;

    public string PartsName => _partsName;
    public GameObject PartsPrefab => _partsPrefab;
    public int RequiredLoad => _requiredLoad;

    public Sprite PartsImage => _partsImage;

    public float MaxDurability => _maxDurability;

    public int Price => _price;

    public string Description => _description;

    public abstract PartsType PartsType { get; }
}

using UnityEngine;

public abstract class PartsData : ScriptableObject
{
    [SerializeField] private string _partsName;
    [SerializeField] private GameObject _partsPrefab;
    [SerializeField] private int _requiredLoad;
    [SerializeField] private Sprite _partsImage;
    [SerializeField] private float _maxDurability;

    public string PartsName => _partsName;
    public GameObject PartsPrefab => _partsPrefab;
    public int RequiredLoad => _requiredLoad;

    public Sprite PartsImage => _partsImage;

    public float MaxDurability => _maxDurability;

    public abstract PartsType PartsType { get; }
}

using UnityEngine;

public abstract class PartsData : ScriptableObject
{
    [SerializeField] private string _partsName;
    [SerializeField] private GameObject _partsPrefab;
    [SerializeField] private int _requiredLoad;

    public string PartsName => _partsName;
    public GameObject PartsPrefab => _partsPrefab;
    public int RequiredLoad => _requiredLoad;

    public abstract PartsType PartsType { get; }
}

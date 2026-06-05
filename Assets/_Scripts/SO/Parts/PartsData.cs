using UnityEngine;

public abstract class PartsData : ScriptableObject
{
    [SerializeField] private string _partsName;
    [SerializeField] private GameObject _partsPrefab;
    [SerializeField] private int _requiredPower;

    public string PartsName => _partsName;
    public GameObject PartsPrefab => _partsPrefab;
    public int RequiredPower => _requiredPower;

    public abstract PartsType PartsType { get; }
}

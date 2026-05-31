using UnityEngine;

public class PartsData : ScriptableObject
{
    [SerializeField] private string _partsName;
    [SerializeField] private GameObject _partsPrefab;
    [SerializeField] private int _requirePower;

    public string PartsName => _partsName;
    public GameObject PartsPrefab => _partsPrefab;
    public int RequirePower => _requirePower;
}

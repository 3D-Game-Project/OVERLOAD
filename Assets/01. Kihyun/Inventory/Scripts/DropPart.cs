using UnityEngine;

[System.Serializable]
public class DropPart
{
    [SerializeField] private PartsData _partsData;
    [Range(0f, 100f)]
    [SerializeField] private float _dropChance = 100f;

    [Header("Durability")]
    [Range(0f, 1f)]
    [SerializeField] private float _minDurabilityRatio = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float _maxDurabilityRatio = 1.0f;

    public PartsData PartsData => _partsData;
    public float DropChance => _dropChance;
    public float MinDurabilityRatio => _minDurabilityRatio;
    public float MaxDurabilityRatio => _maxDurabilityRatio;
}
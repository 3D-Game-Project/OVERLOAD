using UnityEngine;

public abstract class UnitData : ScriptableObject
{
    [Header("공통 기본 정보")]
    [SerializeField] private string _unitName;
    [SerializeField] private int _maxHp;
    // [SerializeField] private List<PartsData> _equippedParts; // 장착된 파츠 정보
    public string UnitName => _unitName;
    public int MaxHp => _maxHp;

}
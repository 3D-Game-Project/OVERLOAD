using UnityEngine;

// 모듈 타입
// 추가할거 있으면 아래로 추가하면 됨
public enum PartType
{
    Core,
    Leg,
    Booster,
    Weapon,
    Armor,
    Utility
}

// 부모 클래스가 될 SO
// 이 SO를 토대로 파츠별로 SO 생성
public abstract class PartDefinition : ScriptableObject
{
    public string partName;
    public PartType partType;

    public float weight;
    public float maxDurability;

    public Sprite icon;
    public GameObject prefab;
}
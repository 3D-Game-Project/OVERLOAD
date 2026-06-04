using UnityEngine;

// 추가 및 수정 필요
public enum LegMovementType
{
    Biped,
    Spider,
    TankWheel,
    BuggyWheel
}

[CreateAssetMenu(menuName = "Parts/Leg Definition")]
public class LegModuleSO: PartDefinition
{
    public float moveSpeed;
    public float acceleration;
    public float decceleration;
    public float turnSpeed;

    // 부스터 없을 때 쓰는 대쉬
    // 삭제할수도 있음
    public float dashPowerCost;
    public float dashForce;

    public LegMovementType movementType;
}
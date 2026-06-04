using UnityEngine;

// 추후 BoosterPart 부분이랑 병합 에정
[CreateAssetMenu(menuName = "Parts/Booster Definition")]
public class BoosterDefinition : PartDefinition
{
    public float jumpForce;
    public float dashForce;
    public float glidePower;
    public float flyPower;

    public float jumpPowerCost;
    public float dashPowerCost;
    public float glidePowerCostPerSecond;
    public float flyPowerCostPerSecond;
}
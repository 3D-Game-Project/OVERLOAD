using UnityEngine;

// 무기 타입
// 추가 및 수정 필요
public enum WeaponFireType
{
    SingleShot,
    AutoFire,
    ChargeShot,
    Missile
}

[CreateAssetMenu(menuName = "Parts/Weapon Definition")]
public class WeaponSO : PartDefinition
{
    public float damage;
    public float fireRate;
    public float range;
    public float projectileSpeed;

    // 출력
    public float powerCostPerShot;

    public WeaponFireType fireType;
}
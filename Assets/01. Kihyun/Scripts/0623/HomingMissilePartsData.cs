using UnityEngine;

[CreateAssetMenu(
    fileName = "New_Homing_Missile_Parts",
    menuName = "Data/Parts/Homing Missile")]
public class HomingMissilePartsData : PartsData
{
    public override PartsType PartsType =>
        PartsType.Utility;

    [Header("Missile")]
    [SerializeField] private GameObject _missilePrefab;
    [SerializeField] private float _damage = 15f;
    [SerializeField] private float _launchInterval = 0.15f;

    public GameObject MissilePrefab => _missilePrefab;
    public float Damage => _damage;
    public float LaunchInterval => _launchInterval;
}
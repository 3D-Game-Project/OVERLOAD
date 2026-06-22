using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Attack_Parts", menuName ="Data/Parts/Attack")]
public class AttackPartsData : PartsData
{
    public override PartsType PartsType => PartsType.Attack;

    [Header("무기 타입")]
    [SerializeField] private FireType _fireType;
    [SerializeField] private WeaponType _weaponType;

    [Header("무기 정보")]
    [SerializeField] private int _damage;
    [SerializeField] private float _range;
    [SerializeField] private float _fireCooldown;
    [SerializeField] private float _fireEnergy;
    [SerializeField] private GameObject _bulletPrefab;

    // 부착시 파워와 소모하는 거를 다르게
    // 별로면 삭제
    [Header("코어 파워 소모")]
    [SerializeField] private float _firePowerCost;

    public int Damage => _damage;
    public float Range => _range;
    public float FireCooldown => _fireCooldown;    

    public float FireEnergy => _fireEnergy;
    public GameObject BulletPrefab => _bulletPrefab;
    public FireType FireType => _fireType;

    public WeaponType WeaponType => _weaponType;
    public float FirePowerCost => _firePowerCost;
}

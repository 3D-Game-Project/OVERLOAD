
using UnityEngine;

[CreateAssetMenu(fileName = "New_Attack_Parts", menuName ="Data/Parts/Attack")]
public class AttackPartsData : PartsData
{
    [Header("무기 타입")]
    [SerializeField] private FireType _fireType;

    [Header("무기 정보")]
    [SerializeField] private int _damage;
    [SerializeField] private float _range;
    [SerializeField] private float _fireCooldown;
    [SerializeField] private int _maxMagazineSize;
    [SerializeField] private GameObject _bulletPrefab;

    public int Damage => _damage;
    public float Range => _range;
    public float FireCooldown => _fireCooldown;    
    public int MaxMagazineSize => _maxMagazineSize;
    public GameObject BulletPrefab => _bulletPrefab;
    public FireType FireType => _fireType;
}

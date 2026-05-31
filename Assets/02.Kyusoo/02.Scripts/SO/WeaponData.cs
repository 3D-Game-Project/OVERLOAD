using UnityEngine;

[CreateAssetMenu(fileName = "New_Weapon_Data", menuName = "Data/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("무기 정보")]
    [SerializeField] private string _weaponName;
    [SerializeField] private GameObject _bulletPrefab;

    [Header("전투 정보")]
    [SerializeField] private int _damage;
    [SerializeField] private float _range;
    [SerializeField] private float _fireCooldown;
    [SerializeField] private int _maxMagazineSize;

    public string WeaponName => _weaponName;
    public GameObject BulletPrefab => _bulletPrefab;
    public int Damage => _damage;
    public float Range => _range;
    public float FireCooldown => _fireCooldown;
    public int MaxMagazineSize => _maxMagazineSize;
}
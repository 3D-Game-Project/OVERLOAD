using UnityEngine;

public class PlayerDefaultAttack : MonoBehaviour
{
    [Header("기본 데이터 설정")]
    [SerializeField] private float _damage = 5f;
    [SerializeField] private float _range = 10f;
    [SerializeField] private float _cooldown = 0.5f;
    [SerializeField] private int _magazineSize = 5;
    [SerializeField] private FireType _fireType = FireType.Projectile;
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private LayerMask _targetLayer;

    [SerializeField] private GameObject _bulletPrefab;

    private PlayerInputHandler _inputHandler;
    private IAimProvider _aimProvider;

    private BulletPool _bulletPool;

    private int _currentAmmo;
    private float _nextFireTime;
    private bool _isReload;
    private bool _isPlayer = false;

    public float Damage => _damage;

    private void Awake()
    {
        if (_fireType == FireType.Projectile && _bulletPrefab != null)
        {
            _bulletPool = gameObject.AddComponent<BulletPool>();
            _bulletPool.Initialize(_bulletPrefab, _magazineSize);
        }
    }

    // 사격 입력 받기 + 좌표받기
    // 총알갯수지정
    private void Start()
    {
        if (gameObject.transform.root.CompareTag("Player"))
        {
            _isPlayer = true;
            _inputHandler = gameObject.transform.root.GetComponent<PlayerInputHandler>();
            _aimProvider = gameObject.transform.root.GetComponentInChildren<IAimProvider>();
        }
        _currentAmmo = _magazineSize;
    }

    // IAimProvider로 좌표받거나 없으면 카메라 기준 정면에 사격범위로 지정
    private void Update()
    {
        if (!_isPlayer || _inputHandler == null) return; 

        if(_inputHandler.IsFire && CanFire())
        {
            Vector3 crosshairTarget = _aimProvider != null ? _aimProvider.GetAimPoint() : Camera.main.transform.position + Camera.main.transform.forward * _range;
            string result = _aimProvider != null ? "_aimProvider 찾음" : "_aimProvider 못찾음";
            Debug.Log(result);
            Fire(crosshairTarget);
        }

        if (_inputHandler.ReloadTriggered)
        {
            StartReload();
        }
    }

    // 공격쿨타임설정
    private bool CanFire()
    {
        return Time.time >= _nextFireTime && _currentAmmo > 0 && !_isReload;
    }

    // 공격처리
    // 총알 소모 쿨타임 재설정
    // 총알생성
    private void Fire(Vector3 targetPoint)
    {
        _currentAmmo--;
        _nextFireTime = Time.time + _cooldown;

        CreateBullet(_muzzlePoint, targetPoint, _damage, _range);

        if (_currentAmmo <= 0) StartReload();
    }

    private void StartReload()
    {
        if (_isReload) return;
        
        _isReload = true;
        Invoke("CompleteReload", 1.0f);
    }

    private void CompleteReload()
    {
        _currentAmmo = _magazineSize;
        _isReload = false;
    }

    /// <summary>
    /// 총알 생성 (기존의 Bullet으로 처리되는것과 유사하게 동작처리하는 대신 Bullet호출말고 자체적으로 구현)
    /// </summary>
    private void CreateBullet(Transform muzzle, Vector3 targetPoint, float dmg, float rng)
    {
        if (muzzle == null) return;

        Vector3 fireDirection = (targetPoint - _muzzlePoint.position).normalized;

        GameObject bulletObj = _bulletPool.GetBullet();
        bulletObj.transform.position = _muzzlePoint.position;

        bulletObj.transform.rotation = Quaternion.LookRotation(fireDirection);
        bulletObj.SetActive(true);

        if (bulletObj.TryGetComponent(out Bullet bullet))
        {
            bullet.Setup((int)_damage, _range, _bulletPool, _targetLayer);
        }
    }
}

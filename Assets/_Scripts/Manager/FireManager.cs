using System.Collections;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    [SerializeField] private AttackPartsData _attackPartsData;
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private LayerMask _targetLayer;

    public WeaponRuntime WeaponRuntime { get; set; }
    private IAimProvider _aimProvider;

    private BulletPool _bulletPool;

    private Coroutine _reloadCoroutine;
    public AttackPartsData AttackPartsData => _attackPartsData;

    // 카메라 세팅 방지
    // 공격파트 정보 수집
    // 발사타입이 Projecttile 일경우 해당 게임오브젝트에 BulletPool 컴포넌트 생성, 프리팹 생성
    // ==> 무기 프리팹마다 BulletPool 스크립트 부착시 발생되는 누락현상 방지와 오브젝트풀링을 통한 총알 사전 생성을 통해 프레임 드랍 방지
    private void Awake()
    {
        if (_attackPartsData != null)
        {
            WeaponRuntime = new WeaponRuntime(_attackPartsData);

            if (_attackPartsData.FireType == FireType.Projectile && _attackPartsData.BulletPrefab != null)
            {
                _bulletPool = gameObject.AddComponent<BulletPool>();
                _bulletPool.Initialize(_attackPartsData.BulletPrefab, _attackPartsData.MaxMagazineSize);
            }
        }
    }

    private void Start()
    {
        _aimProvider = GetComponentInParent<IAimProvider>();
    }

    // FireManager를 Enemy와 Player 둘 다가 사용할 예정이기 때문에
    // CompareTag를 통해 플레이어인 경우에만 마우스 클릭으로 발사 및 리로드 처리
    private void Update()
    {
        if (WeaponRuntime == null) return;

        if (gameObject.transform.root.CompareTag("Player"))
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 crosshairTarget = _aimProvider != null ? _aimProvider.GetAimPoint() : Camera.main.transform.position + Camera.main.transform.forward * _attackPartsData.Range;
                Debug.Log($"[사격 시도] 조준점 좌표: {crosshairTarget}");
                TryFire(crosshairTarget);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                WeaponRuntime.StartReload();
            }
        }
    }

    private void OnEnable()
    {
        if (WeaponRuntime != null)
        {
            WeaponRuntime.OnReloadStarted += ReloadStarted;
        }
    }

    private void OnDisable()
    {
        if (WeaponRuntime != null)
        {
            WeaponRuntime.OnReloadStarted -= ReloadStarted;
        }
    }

    public void TryFire(Vector3 targetPoint)
    {
        if (WeaponRuntime.TryFire())
        {
            //RotateToPlayer(targetPoint);


            switch (_attackPartsData.FireType)
            {
                case FireType.Projectile:
                    CreateBullet(targetPoint);
                    break;
                case FireType.Hitscan:
                    FireHitscan(targetPoint);
                    break;
            }
        }
    }

    // 총알 생성 (ProjectTile은 해당 방향으로 발사되도록 처리해야하기 때문에)
    // Target의 위치와 총구의 위치로 사격할 방향 계산
    // 총알 생성은 사전에 오브젝트 풀링을 통해 생성한 총알 가져오기
    // 총알의 조준방향은 발사방향을 기준으로 정면으로 나가도록 즉각적인 각도 조정(위 혹은 아래의 적을 조준할 때 총알이 꺽여나가게하지 않도록)
    // 마지막 조건문은 생성되는 총알의 데미지, 사거리 정보를 추가
    private void CreateBullet(Vector3 targetPoint)
    {
        if (_bulletPool == null) return;

        Vector3 fireDirection = (targetPoint - _muzzlePoint.position).normalized;

        GameObject bulletObj = _bulletPool.GetBullet();
        bulletObj.transform.position = _muzzlePoint.position;

        bulletObj.transform.rotation = Quaternion.LookRotation(fireDirection);
        bulletObj.SetActive(true);

        if (bulletObj.TryGetComponent(out Bullet bullet))
        {
            bullet.Setup(_attackPartsData.Damage, _attackPartsData.Range, _bulletPool, _targetLayer);
        }
    }

    // 히트스캔타입의 무기 전용 발사 로직
    // 발사방향 설정 후, Ray를 쏴서 히트된 정보 판별
    // Enemy, Player모두 FireManager를 사용할 예정이기 때문에 각각의 피격 함수 호출
    private void FireHitscan(Vector3 targetPoint)
    {
        Debug.Log($"HitScan 호출됨");
        Vector3 _fireDirection = (targetPoint - _muzzlePoint.position).normalized;

        if (Physics.Raycast(_muzzlePoint.position, _fireDirection, out RaycastHit _hit, _attackPartsData.Range, _targetLayer))
        {

            Debug.Log($"FindHitScan {_hit.collider.name}");

            EnemyCombatController enemy = _hit.collider.GetComponentInParent<EnemyCombatController>();
            PlayerCombatController player = _hit.collider.GetComponentInParent<PlayerCombatController>();

            if (enemy != null)
            {
                enemy.TakeDamage(_attackPartsData.Damage);
            }
            else if (player != null)
            {
                Debug.DrawLine(_muzzlePoint.position, _hit.point, Color.black, 0.5f);
                Debug.Log($"<color=red>[Hitscan Hit] 플레이어 타격 성공!</color> 데미지: {_attackPartsData.Damage} | 컴포넌트 오브젝트: {player.gameObject.name}");

                player.TakeDamage(_attackPartsData.Damage);
            }

        }
    }

    // 동적 장착 시 무기 파츠의 소유자 정보(레이어 및 타겟) 및 데이터 런타임 초기화
    // 인스펙터에 총구 정보가 누락되었을 시 자식 오브젝트 트리를 자동 탐색하여 보정 처리
    // 소유자(Player/Enemy)기준  레이어 설정
    // 무기 교체 타이밍에 맞춰 기존에 생성되어 있던 탄환 오브젝트 풀을 완전 청소(Destroy)한 뒤, 새 무기 스펙 탄창 수량에 맞게 재구축
    public void SetWeaponOwner(LayerMask targetLayer, int ownerLayer)
    {
        if (_muzzlePoint == null)
        {
            _muzzlePoint = FindChildMuzzle(transform, "MuzzlePoint");

            if (_muzzlePoint != null)
            {
                Debug.Log($"muzzlePoint 찾기 성공.");
            }
            else
            {
                _muzzlePoint = transform;
            }
        }

        _targetLayer = targetLayer;
        gameObject.layer = ownerLayer;

        foreach (Transform child in transform)
        {
            child.gameObject.layer = ownerLayer;
        }

        _aimProvider = GetComponentInParent<IAimProvider>();

        if (_attackPartsData != null)
        {
            WeaponRuntime = new WeaponRuntime(_attackPartsData);

            if (_attackPartsData.FireType == FireType.Projectile && _attackPartsData.BulletPrefab != null)
            {
                if (TryGetComponent(out BulletPool oldPool)) Destroy(oldPool);

                _bulletPool = gameObject.AddComponent<BulletPool>();
                _bulletPool.Initialize(_attackPartsData.BulletPrefab, _attackPartsData.MaxMagazineSize);
            }
        }
    }

    // 총구 위치 자동 탐색
    // 하이어라키 하위 구조 기준 재귀 함수 형태로 순회하여 MuzzlePoint 문자열로 찾아서 연결
    private Transform FindChildMuzzle(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == targetName)
                return child;

            Transform found = FindChildMuzzle(child, targetName);
            if (found != null)
                return found;
        }
        return null;
    }

    // 무기 재장전시 동작되는 코루틴
    private void ReloadStarted()
    {
        if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
        _reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    // 2초 대기 후 Reload완료되었다고 알리기
    private IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(2f);

        WeaponRuntime.CompleteReload();
        _reloadCoroutine = null;
    }

    // 몬스터 기준 플레이어가 후방에 있어도 총알이 뒤로 발사되지 않도록 플레이어 방향으로 회전시키는 함수
    // lookDirection.y = 0f;를 통해 몬스터가 상하로 회전하지않도록 처리
    //private void RotateToPlayer(Vector3 targetPoint)
    //{
    //    if (gameObject.transform.root == null || !gameObject.transform.root.CompareTag("Enemy")) return;
    //    Transform rootTransform = gameObject.transform.root;

    //    Vector3 lookDirection = targetPoint - rootTransform.position;
    //    lookDirection.y = 0f;

    //    if (lookDirection != Vector3.zero) rootTransform.rotation = Quaternion.Slerp(rootTransform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
    //}
}

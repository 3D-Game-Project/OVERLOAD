using System.Collections;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    [SerializeField] private AttackPartsData _attackPartsData;
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private LayerMask _targetLayer;

    public WeaponRuntime WeaponRuntime { get; private set; }

    private BulletPool _bulletPool;
    private Coroutine _reloadCoroutine;
    private bool _isReloadEventSubscribed;

    public AttackPartsData AttackPartsData => _attackPartsData;

    // 카메라 세팅 방지
    // 공격파트 정보 수집
    // 발사타입이 Projecttile 일경우 해당 게임오브젝트에 BulletPool 컴포넌트 생성, 프리팹 생성
    // ==> 무기 프리팹마다 BulletPool 스크립트 부착시 발생되는 누락현상 방지와 오브젝트풀링을 통한 총알 사전 생성을 통해 프레임 드랍 방지
    private void Awake()
    {
        EnsureMuzzlePoint();

        if (_attackPartsData != null)
        {
            RebuildWeaponRuntime();
            RebuildBulletPool();
        }
    }

    private void OnEnable()
    {
        SubscribeReloadEvent();
    }

    private void OnDisable()
    {
        UnsubscribeReloadEvent();
    }

    // FireManager를 Enemy와 Player 둘 다가 사용할 예정이기 때문에
    // 기존에는 CompareTag를 통해 플레이어인 경우에만 마우스 클릭으로 발사 및 리로드 처리
    // 현재 구조에서는 FireManager가 직접 입력을 읽지 않음
    // 플레이어 입력은 CorePartsController / AttackPartController 쪽에서 처리하고,
    // FireManager는 외부에서 TryFire()를 호출받아 실제 발사만 담당
    // 즉, 기존 Update 입력 처리 로직은 제거
    /*
    private void Update()
    {
        if (WeaponRuntime == null) return;

        if (_isPlayer && _playerInputHandler != null) 
        {
            if (_playerInputHandler.IsFire)
            {
                Vector3 crosshairTarget = _aimProvider != null ? _aimProvider.GetAimPoint() : Camera.main.transform.position + Camera.main.transform.forward * _attackPartsData.Range;
                TryFire(crosshairTarget);
            }

            if (_playerInputHandler.ReloadTriggered)
            {
                _playerInputHandler.ReloadTriggered = false;

                WeaponRuntime.StartReload();
            }
        }
    }
    */

    // 우리 CorePartsController / AttackPartController용 초기화 함수
    // 외부에서 AttackPartsData까지 넘겨받아 무기 상태를 새로 설정
    public void InitializeWeapon(
        AttackPartsData attackPartsData,
        LayerMask targetLayer,
        int ownerLayer)
    {
        _attackPartsData = attackPartsData;

        SetWeaponOwner(targetLayer, ownerLayer);
    }

    public void TryFire(Vector3 targetPoint)
    {
        if (_attackPartsData == null)
            return;

        if (WeaponRuntime == null)
            return;

        if (_muzzlePoint == null)
            return;

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

    public void StartReload()
    {
        if (WeaponRuntime == null)
            return;

        WeaponRuntime.StartReload();
    }

    // 총알 생성 (ProjectTile은 해당 방향으로 발사되도록 처리해야하기 때문에)
    // Target의 위치와 총구의 위치로 사격할 방향 계산
    // 총알 생성은 사전에 오브젝트 풀링을 통해 생성한 총알 가져오기
    // 총알의 조준방향은 발사방향을 기준으로 정면으로 나가도록 즉각적인 각도 조정(위 혹은 아래의 적을 조준할 때 총알이 꺽여나가게하지 않도록)
    // 마지막 조건문은 생성되는 총알의 데미지, 사거리 정보를 추가
    private void CreateBullet(Vector3 targetPoint)
    {
        if (_bulletPool == null)
            return;

        Vector3 fireDirection = (targetPoint - _muzzlePoint.position).normalized;

        GameObject bulletObj = _bulletPool.GetBullet();

        bulletObj.transform.position = _muzzlePoint.position;
        bulletObj.transform.rotation = Quaternion.LookRotation(fireDirection);
        bulletObj.SetActive(true);

        if (bulletObj.TryGetComponent(out Bullet bullet))
        {
            bullet.Setup(
                _attackPartsData.Damage,
                _attackPartsData.Range,
                _bulletPool,
                _targetLayer
            );
        }
    }

    // 히트스캔타입의 무기 전용 발사 로직
    // 발사방향 설정 후, Ray를 쏴서 히트된 정보 판별
    // Enemy, Player모두 FireManager를 사용할 예정이기 때문에 각각의 피격 함수 호출
    private void FireHitscan(Vector3 targetPoint)
    {
        Debug.Log($"HitScan 호출됨");

        Vector3 fireDirection = (targetPoint - _muzzlePoint.position).normalized;

        if (Physics.Raycast(
                _muzzlePoint.position,
                fireDirection,
                out RaycastHit hit,
                _attackPartsData.Range,
                _targetLayer))
        {
            Debug.Log($"FindHitScan {hit.collider.name}");

            DurabilityController hitDurability = hit.collider.GetComponentInParent<DurabilityController>();

            if (hitDurability != null)
        {
            // 🎯 맞은 부위가 오른팔이면 오른팔 스크립트의 TakeDamage가 실행되어
            // 알아서 오른팔 내구도가 깎이고, 오른팔 방어구를 추적하게 됩니다!
            hitDurability.TakeDamage(_attackPartsData.Damage);
            
            Debug.Log($"Hitscan 부위 이름: {hit.collider.gameObject.name} / 타입: {hitDurability.DurabilityType}");
        }
        }
    }

    // 동적 장착 시 무기 파츠의 소유자 정보(레이어 및 타겟) 및 데이터 런타임 초기화
    // 인스펙터에 총구 정보가 누락되었을 시 자식 오브젝트 트리를 자동 탐색하여 보정 처리
    // 소유자(Player/Enemy)기준  레이어 설정
    // 무기 교체 타이밍에 맞춰 기존에 생성되어 있던 탄환 오브젝트 풀을 완전 청소(Destroy)한 뒤, 새 무기 스펙 탄창 수량에 맞게 재구축
    public void SetWeaponOwner(LayerMask targetLayer, int ownerLayer)
    {
        _targetLayer = targetLayer;

        EnsureMuzzlePoint();
        SetLayerRecursively(transform, ownerLayer);

        if (_attackPartsData == null)
        {
            Debug.LogWarning($"{gameObject.name}에 AttackPartsData가 없습니다.");
            return;
        }

        RebuildWeaponRuntime();
        RebuildBulletPool();
    }

    private void RebuildWeaponRuntime()
    {
        if (_attackPartsData == null)
            return;

        StopReloadCoroutine();
        UnsubscribeReloadEvent();

        WeaponRuntime = new WeaponRuntime(_attackPartsData);
        _isReloadEventSubscribed = false;

        if (isActiveAndEnabled)
        {
            SubscribeReloadEvent();
        }
    }

    private void RebuildBulletPool()
    {
        if (_bulletPool != null)
        {
            Destroy(_bulletPool);
            _bulletPool = null;
        }

        if (_attackPartsData == null)
            return;

        if (_attackPartsData.FireType != FireType.Projectile)
            return;

        if (_attackPartsData.BulletPrefab == null)
            return;

        int poolSize = Mathf.Max(1, _attackPartsData.MaxMagazineSize);

        _bulletPool = gameObject.AddComponent<BulletPool>();
        _bulletPool.Initialize(
            _attackPartsData.BulletPrefab,
            poolSize
        );
    }

    private void SubscribeReloadEvent()
    {
        if (WeaponRuntime == null)
            return;

        if (_isReloadEventSubscribed)
            return;

        WeaponRuntime.OnReloadStarted += ReloadStarted;
        _isReloadEventSubscribed = true;
    }

    private void UnsubscribeReloadEvent()
    {
        if (WeaponRuntime == null)
            return;

        if (!_isReloadEventSubscribed)
            return;

        WeaponRuntime.OnReloadStarted -= ReloadStarted;
        _isReloadEventSubscribed = false;
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

    private void EnsureMuzzlePoint()
    {
        if (_muzzlePoint != null)
            return;

        _muzzlePoint = FindChildMuzzle(transform, "MuzzlePoint");

        if (_muzzlePoint != null)
        {
            Debug.Log($"muzzlePoint 찾기 성공.");
            return;
        }

        Debug.LogWarning(
            $"{gameObject.name}에 MuzzlePoint가 없어 transform을 총구로 사용합니다."
        );

        _muzzlePoint = transform;
    }

    private void SetLayerRecursively(Transform target, int layer)
    {
        target.gameObject.layer = layer;

        foreach (Transform child in target)
        {
            SetLayerRecursively(child, layer);
        }
    }

    // 무기 재장전시 동작되는 코루틴
    private void ReloadStarted()
    {
        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
        }

        _reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    // 2초 대기 후 Reload완료되었다고 알리기
    private IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(2f);

        if (WeaponRuntime != null)
        {
            WeaponRuntime.CompleteReload();
        }

        _reloadCoroutine = null;
    }

    private void StopReloadCoroutine()
    {
        if (_reloadCoroutine == null)
            return;

        StopCoroutine(_reloadCoroutine);
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
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingMissileProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _initialSpeed = 12f;
    [SerializeField] private float _maxSpeed = 26f;
    [SerializeField] private float _acceleration = 14f;
    [SerializeField] private float _linearDamping = 0.5f;

    [Header("Homing Turn")]
    [SerializeField] private float _cruiseTurnSpeed = 140f;
    [SerializeField] private float _terminalTurnSpeed = 30f;
    [SerializeField] private float _terminalDistance = 7f;
    [SerializeField] private float _fullTrackingDistance = 18f;

    [Header("Launch Arc")]
    [SerializeField] private float _ascentDuration = 0.45f;
    [SerializeField] private float _upwardWeight = 1.2f;

    [Header("Evasion")]
    [SerializeField] private float _missGiveUpDistance = 2.5f;
    [SerializeField] private float _fallTurnSpeed = 35f;

    [Header("Lifetime")]
    [SerializeField] private float _homingDuration = 5f;
    [SerializeField] private float _lifeTime = 8f;

    [Header("Target")]
    [SerializeField]
    private Vector3 _targetOffset =
        new Vector3(0f, 1f, 0f);

    private Rigidbody _rigidbody;

    private Transform _target;
    private Transform _ownerRoot;

    private LayerMask _targetLayer;
    private float _damage;
    private float _elapsedTime;

    private bool _isLaunched;
    private bool _hasMissedTarget;

    private Vector3 _ascentDirection;
    private float _closestTargetDistance;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _rigidbody.useGravity = false;
        _rigidbody.linearDamping = _linearDamping;

        _rigidbody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        _rigidbody.interpolation =
            RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        if (!_isLaunched)
            return;

        _elapsedTime += Time.fixedDeltaTime;

        if (_elapsedTime >= _lifeTime)
        {
            DestroyMissile();
            return;
        }

        Vector3 currentVelocity =
            _rigidbody.linearVelocity;

        Vector3 currentDirection =
            currentVelocity.sqrMagnitude > 0.001f
                ? currentVelocity.normalized
                : transform.forward;

        Vector3 desiredDirection = currentDirection;

        float appliedTurnSpeed =
            _cruiseTurnSpeed;

        // 발사 직후에는 플레이어를 추적하지 않고 상승한다.
        if (_elapsedTime < _ascentDuration)
        {
            desiredDirection = _ascentDirection;
        }
        else if (_target != null &&
                 !_hasMissedTarget &&
                 _elapsedTime <
                 _ascentDuration + _homingDuration)
        {
            CalculateHomingDirection(
                currentDirection,
                ref desiredDirection,
                ref appliedTurnSpeed
            );
        }
        else
        {
            // 추적 종료 또는 회피 성공 후 아래로 낙하한다.
            desiredDirection = Vector3.down;
            appliedTurnSpeed = _fallTurnSpeed;
        }

        Vector3 newDirection =
            Vector3.RotateTowards(
                currentDirection,
                desiredDirection,
                appliedTurnSpeed *
                Mathf.Deg2Rad *
                Time.fixedDeltaTime,
                0f
            ).normalized;

        float newSpeed = Mathf.MoveTowards(
            currentVelocity.magnitude,
            _maxSpeed,
            _acceleration * Time.fixedDeltaTime
        );

        _rigidbody.linearVelocity =
            newDirection * newSpeed;

        if (_rigidbody.linearVelocity.sqrMagnitude >
            0.001f)
        {
            transform.rotation = Quaternion.LookRotation(
                _rigidbody.linearVelocity.normalized
            );
        }
    }

    private void CalculateHomingDirection(
        Vector3 currentDirection,
        ref Vector3 desiredDirection,
        ref float appliedTurnSpeed)
    {
        Vector3 targetPosition =
            _target.position + _targetOffset;

        Vector3 toTarget =
            targetPosition - transform.position;

        float distance = toTarget.magnitude;

        if (distance <= 0.001f)
            return;

        Vector3 targetDirection =
            toTarget / distance;

        // 멀리서는 강하게 추적하고 가까이에서는
        // 선회력을 낮춰 대쉬로 피할 수 있게 한다.
        float distanceFactor = Mathf.InverseLerp(
            _terminalDistance,
            _fullTrackingDistance,
            distance
        );

        appliedTurnSpeed = Mathf.Lerp(
            _terminalTurnSpeed,
            _cruiseTurnSpeed,
            distanceFactor
        );

        _closestTargetDistance = Mathf.Min(
            _closestTargetDistance,
            distance
        );

        // 근접한 뒤 타겟이 뒤로 넘어가면
        // 회피에 성공한 것으로 판단한다.
        if (_closestTargetDistance <=
                _missGiveUpDistance &&
            Vector3.Dot(
                currentDirection,
                targetDirection) < 0f)
        {
            _hasMissedTarget = true;
            return;
        }

        desiredDirection = targetDirection;
    }

    public void Launch(
        Transform target,
        Transform ownerRoot,
        float damage,
        LayerMask targetLayer,
        Vector3 launchDirection)
    {
        _target = target;
        _ownerRoot = ownerRoot;

        _damage = damage;
        _targetLayer = targetLayer;

        _elapsedTime = 0f;
        _hasMissedTarget = false;
        _closestTargetDistance = float.MaxValue;

        if (launchDirection.sqrMagnitude < 0.001f)
            launchDirection = transform.forward;

        launchDirection.Normalize();

        _ascentDirection =
            (launchDirection +
             Vector3.up * _upwardWeight).normalized;

        transform.rotation =
            Quaternion.LookRotation(_ascentDirection);

        _rigidbody.linearVelocity =
            _ascentDirection * _initialSpeed;

        _isLaunched = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isLaunched || other == null)
            return;

        // 발사한 보스의 콜라이더는 무시한다.
        if (_ownerRoot != null &&
            other.transform.IsChildOf(_ownerRoot))
        {
            return;
        }

        int otherLayer = other.gameObject.layer;

        bool isTargetLayer =
            (_targetLayer.value &
             (1 << otherLayer)) != 0;

        if (isTargetLayer)
        {
            DurabilityController durability =
                other.GetComponentInParent<
                    DurabilityController>();

            if (durability != null)
            {
                durability.TakeDamage(_damage);
                DestroyMissile();
            }

            return;
        }

        // 지형이나 다른 고체 오브젝트와 충돌하면 제거한다.
        if (!other.isTrigger)
        {
            DestroyMissile();
        }
    }

    private void DestroyMissile()
    {
        if (!_isLaunched)
            return;

        _isLaunched = false;
        _rigidbody.linearVelocity = Vector3.zero;

        Destroy(gameObject);
    }
}
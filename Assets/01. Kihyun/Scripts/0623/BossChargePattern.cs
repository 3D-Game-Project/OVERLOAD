using System.Collections;
using UnityEngine;

public class BossChargePattern : BossAttackPatternBase
{
    [Header("References")]
    [SerializeField] private CorePartsController _corePartsController;
    [SerializeField] private BossMovementController _movementController;
    [SerializeField] private Transform _chargeOrigin;

    [Header("Aiming")]
    [SerializeField] private float _aimDuration = 1.2f;

    [Header("Charge")]
    [SerializeField] private float _chargeSpeed = 20f;
    [SerializeField] private float _chargeDuration = 1.2f;
    [SerializeField] private float _hitRadius = 1.5f;
    [SerializeField] private float _damage = 40f;
    [SerializeField] private LayerMask _targetLayer;

    [Header("Recovery")]
    [SerializeField] private float _recoveryDuration = 1f;

    private Vector3 _chargeDirection;

    public override BossAttackPatternType PatternType =>
        BossAttackPatternType.Charge;

    public override bool CanRunWhileMoving => false;

    private void Awake()
    {
        if (_corePartsController == null)
        {
            _corePartsController =
                GetComponentInParent<CorePartsController>();
        }

        if (_movementController == null)
        {
            _movementController =
                GetComponentInParent<BossMovementController>();
        }

        if (_chargeOrigin == null)
            _chargeOrigin = transform;
    }

    protected override bool CheckRequirements(Transform target)
    {
        return
            _corePartsController != null &&
            _movementController != null &&
            _corePartsController
                .HasOperationalPart<IBoosterPart>();
    }

    protected override IEnumerator ExecutePattern(Transform target)
    {
        _movementController.StopMovement();

        // 조준 단계
        float aimTimer = 0f;

        while (aimTimer < _aimDuration)
        {
            if (ShouldStop(target))
                yield break;

            if (!HasOperationalBooster())
            {
                RequestCancel();
                yield break;
            }

            _movementController.StopMovement();
            _movementController.LookAtTarget(target.position);

            aimTimer += Time.deltaTime;
            yield return null;
        }

        // 이 순간의 보스 정면으로 돌진 방향 고정
        _chargeDirection = Vector3.ProjectOnPlane(
            _movementController.transform.forward,
            Vector3.up
        ).normalized;

        if (_chargeDirection.sqrMagnitude < 0.001f)
        {
            RequestCancel();
            yield break;
        }

        // 돌진 중에는 플레이어를 더 이상 추적하지 않는다.
        _movementController.ClearLookTarget();
        _movementController.SetSpeed(_chargeSpeed);

        float chargeTimer = 0f;

        while (chargeTimer < _chargeDuration)
        {
            if (ShouldStop(target))
                yield break;

            if (!HasOperationalBooster())
            {
                RequestCancel();
                yield break;
            }

            float moveDistance =
                _chargeSpeed * Time.deltaTime;

            if (TryHitPlayer(moveDistance))
                break;

            Vector3 destination =
                _movementController.transform.position +
                _chargeDirection * 10f;

            _movementController.MoveToTarget(destination);

            chargeTimer += Time.deltaTime;
            yield return null;
        }

        StopCharge();

        float recoveryTimer = 0f;

        while (recoveryTimer < _recoveryDuration)
        {
            if (ShouldStop(target))
                yield break;

            _movementController.StopMovement();

            recoveryTimer += Time.deltaTime;
            yield return null;
        }
    }

    protected override void OnPatternEnded(bool wasCancelled)
    {
        StopCharge();
    }

    private bool TryHitPlayer(float moveDistance)
    {
        if (!Physics.SphereCast(
                _chargeOrigin.position,
                _hitRadius,
                _chargeDirection,
                out RaycastHit hit,
                moveDistance,
                _targetLayer,
                QueryTriggerInteraction.Collide))
        {
            return false;
        }

        DurabilityController durability =
            hit.collider.GetComponentInParent<DurabilityController>();

        if (durability != null)
        {
            durability.TakeDamage(_damage);

            Debug.Log(
                $"돌진 충돌: {durability.gameObject.name}"
            );
        }

        return true;
    }

    private bool HasOperationalBooster()
    {
        return _corePartsController != null &&
               _corePartsController
                   .HasOperationalPart<IBoosterPart>();
    }

    private void StopCharge()
    {
        if (_movementController == null)
            return;

        _movementController.StopMovement();
        _movementController.ClearLookTarget();
        _movementController.ResetSpeed();
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin =
            _chargeOrigin != null
                ? _chargeOrigin
                : transform;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            origin.position,
            _hitRadius
        );
    }
}
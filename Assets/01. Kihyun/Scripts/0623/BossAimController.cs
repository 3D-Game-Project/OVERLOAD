using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BossAimController :
    MonoBehaviour,
    IAimStateProvider
{
    [Header("References")]
    [SerializeField] private Transform _aimOrigin;
    [SerializeField] private Transform _coreYawRoot;

    [Header("Aim")]
    [SerializeField] private float _trackingSpeed = 60f;
    [SerializeField] private float _aimDistance = 100f;
    [SerializeField] private Vector3 _targetOffset;
    [SerializeField] private float _coreForwardYawOffset;

    [Header("Ray")]
    [SerializeField] private LayerMask _lineHitMask;

    [Header("Aim Assist")]
    [SerializeField] private LayerMask _aimAssistMask;
    [SerializeField] private float _aimAssistRadius = 0.75f;

    private LineRenderer _lineRenderer;
    private Transform _target;
    private Vector3 _currentAimDirection;

    public bool IsAiming { get; private set; }

    public Vector3 CurrentAimDirection =>
        _currentAimDirection;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.enabled = false;

        if (_aimOrigin == null)
            _aimOrigin = transform;

        _currentAimDirection = _aimOrigin.forward;
    }

    private void LateUpdate()
    {
        if (!IsAiming)
            return;

        if (_target == null)
        {
            StopAiming();
            return;
        }

        UpdateAimDirection();
        ApplyCoreYaw();
        UpdateAimLine();
    }

    public void StartAiming(Transform target)
    {
        if (target == null)
            return;

        _target = target;
        IsAiming = true;
        _lineRenderer.enabled = true;

        if (_currentAimDirection.sqrMagnitude < 0.001f)
            _currentAimDirection = _aimOrigin.forward;

        UpdateAimLine();
    }

    public void StopAiming()
    {
        _target = null;
        IsAiming = false;

        if (_lineRenderer != null)
            _lineRenderer.enabled = false;
    }

    public Vector3 GetAimPoint()
    {
        if (_aimOrigin == null)
        {
            return transform.position +
                   transform.forward * _aimDistance;
        }

        Vector3 direction =
            _currentAimDirection.sqrMagnitude > 0.001f
                ? _currentAimDirection.normalized
                : _aimOrigin.forward;

        return _aimOrigin.position +
               direction * _aimDistance;
    }

    private void UpdateAimDirection()
    {
        Vector3 targetPosition =
            _target.position + _targetOffset;

        Vector3 desiredDirection =
            targetPosition - _aimOrigin.position;

        if (desiredDirection.sqrMagnitude < 0.001f)
            return;

        desiredDirection.Normalize();

        _currentAimDirection = Vector3.RotateTowards(
            _currentAimDirection,
            desiredDirection,
            _trackingSpeed *
            Mathf.Deg2Rad *
            Time.deltaTime,
            0f
        ).normalized;
    }

    private void ApplyCoreYaw()
    {
        if (_coreYawRoot == null)
            return;

        Vector3 flatDirection =
            Vector3.ProjectOnPlane(
                _currentAimDirection,
                Vector3.up
            );

        if (flatDirection.sqrMagnitude < 0.001f)
            return;

        float targetYaw =
            Quaternion.LookRotation(flatDirection)
                .eulerAngles.y;

        _coreYawRoot.rotation = Quaternion.Euler(
            0f,
            targetYaw + _coreForwardYawOffset,
            0f
        );
    }

    private void UpdateAimLine()
    {
        if (_lineRenderer == null ||
            _aimOrigin == null)
        {
            return;
        }

        Vector3 startPoint = _aimOrigin.position;
        Vector3 direction = _currentAimDirection.normalized;

        Vector3 endPoint =
            startPoint + direction * _aimDistance;

        if (TryGetAssistedTarget(
                out _,
                out Vector3 assistedPoint))
        {
            endPoint = assistedPoint;
        }
        else if (Physics.Raycast(
                     startPoint,
                     direction,
                     out RaycastHit hit,
                     _aimDistance,
                     _lineHitMask,
                     QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;
        }

        _lineRenderer.SetPosition(0, startPoint);
        _lineRenderer.SetPosition(1, endPoint);
    }

    public bool TryGetAssistedTarget(
    out DurabilityController targetPart,
    out Vector3 hitPoint)
    {
        targetPart = null;
        hitPoint = Vector3.zero;

        if (!IsAiming || _aimOrigin == null)
            return false;

        Vector3 origin = _aimOrigin.position;
        Vector3 direction = _currentAimDirection.normalized;

        // 레이 중앙에 직접 닿은 파츠를 우선한다.
        bool hasDirectHit = Physics.Raycast(
            origin,
            direction,
            out RaycastHit directHit,
            _aimDistance,
            _lineHitMask,
            QueryTriggerInteraction.Ignore
        );

        if (hasDirectHit)
        {
            DurabilityController directTarget =
                directHit.collider
                    .GetComponentInParent<DurabilityController>();

            if (IsValidAimTarget(directTarget))
            {
                targetPart = directTarget;
                hitPoint = directHit.point;
                return true;
            }
        }

        // 레이 주변에 들어온 플레이어 파츠를 탐색한다.
        RaycastHit[] assistHits = Physics.SphereCastAll(
            origin,
            _aimAssistRadius,
            direction,
            _aimDistance,
            _aimAssistMask,
            QueryTriggerInteraction.Collide
        );

        float nearestDistance = float.MaxValue;

        foreach (RaycastHit assistHit in assistHits)
        {
            DurabilityController durability =
                assistHit.collider
                    .GetComponentInParent<DurabilityController>();

            if (!IsValidAimTarget(durability))
                continue;

            // 중앙 레이보다 먼저 벽이나 지형에 막혔다면 조준하지 않는다.
            if (hasDirectHit &&
                directHit.distance < assistHit.distance)
            {
                continue;
            }

            if (assistHit.distance >= nearestDistance)
                continue;

            nearestDistance = assistHit.distance;
            targetPart = durability;
            hitPoint = assistHit.collider.bounds.center;
        }

        return targetPart != null;
    }

    private bool IsValidAimTarget(
        DurabilityController durability)
    {
        if (durability == null)
            return false;

        if (durability.IsDestroyed ||
            durability.CurrentDurability <= 0f)
        {
            return false;
        }

        if (_target == null)
            return true;

        return durability.transform == _target ||
               durability.transform.IsChildOf(_target);
    }
}
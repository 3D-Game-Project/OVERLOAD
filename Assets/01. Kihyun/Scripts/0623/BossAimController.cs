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
        Vector3 direction =
            _currentAimDirection.normalized;

        Vector3 endPoint =
            startPoint + direction * _aimDistance;

        if (Physics.Raycast(
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
}
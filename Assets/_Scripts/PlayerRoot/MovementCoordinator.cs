using UnityEngine;

public class MovementCoordinator : MonoBehaviour
{
    [Header("Motor")]
    [SerializeField] private PlayerLocomotionMotor _locomotionMotor;

    private Vector3 _groundHorizontalVelocitySum;
    private int _groundRequestCount;

    private Vector3 _additionalHorizontalVelocity;

    private float _verticalVelocityChange;

    private bool _hasJumpRequest;
    private float _requestedJumpVelocity;

    private bool _hasFallSpeedLimitRequest;
    private float _requestedFallSpeedLimit;

    private float _requestedHorizontalSpeedMultiplier = 1f;

    private bool _isDashBoostingThisFrame;
    private bool _wasDashBoostingLastFrame;

    public bool IsDashBoostingThisFrame => _isDashBoostingThisFrame;
    public bool WasDashBoostingLastFrame => _wasDashBoostingLastFrame;

    private void Awake()
    {
        if (_locomotionMotor == null)
            _locomotionMotor = GetComponent<PlayerLocomotionMotor>();
    }

    public void BeginFrame()
    {
        _wasDashBoostingLastFrame = _isDashBoostingThisFrame;
        _isDashBoostingThisFrame = false;

        _groundHorizontalVelocitySum = Vector3.zero;
        _groundRequestCount = 0;

        _additionalHorizontalVelocity = Vector3.zero;

        _verticalVelocityChange = 0f;

        _hasJumpRequest = false;
        _requestedJumpVelocity = 0f;

        _hasFallSpeedLimitRequest = false;
        _requestedFallSpeedLimit = 0f;

        _requestedHorizontalSpeedMultiplier = 1f;
    }

    public void SubmitGroundHorizontalVelocity(Vector3 velocity)
    {
        velocity.y = 0f;

        _groundHorizontalVelocitySum += velocity;
        _groundRequestCount++;
    }

    public void SubmitAdditionalHorizontalVelocity(Vector3 velocity)
    {
        velocity.y = 0f;

        _additionalHorizontalVelocity += velocity;
    }

    public void NotifyDashBoosting()
    {
        _isDashBoostingThisFrame = true;
    }

    public void SubmitVerticalVelocityChange(float velocityChange)
    {
        _verticalVelocityChange += velocityChange;
    }

    public void SubmitVerticalAcceleration(float acceleration, float deltaTime)
    {
        _verticalVelocityChange += acceleration * deltaTime;
    }

    public void RequestJump(float jumpVelocity)
    {
        _hasJumpRequest = true;
        _requestedJumpVelocity = Mathf.Max(_requestedJumpVelocity, jumpVelocity);
    }

    public void RequestFallSpeedLimit(float maxFallSpeed)
    {
        if (!_hasFallSpeedLimitRequest)
        {
            _requestedFallSpeedLimit = maxFallSpeed;
            _hasFallSpeedLimitRequest = true;
            return;
        }

        _requestedFallSpeedLimit = Mathf.Max(_requestedFallSpeedLimit, maxFallSpeed);
    }

    public void RequestHorizontalSpeedMultiplier(float multiplier)
    {
        _requestedHorizontalSpeedMultiplier =
            Mathf.Max(_requestedHorizontalSpeedMultiplier, multiplier);
    }

    public void ApplyFrame()
    {
        if (_locomotionMotor == null)
            return;

        Vector3 finalHorizontalVelocity = Vector3.zero;

        if (_groundRequestCount > 0)
        {
            finalHorizontalVelocity =
                _groundHorizontalVelocitySum / _groundRequestCount;
        }

        finalHorizontalVelocity += _additionalHorizontalVelocity;

        _locomotionMotor.SetHorizontalVelocity(finalHorizontalVelocity);

        if (_hasJumpRequest)
        {
            _locomotionMotor.ApplyJump(_requestedJumpVelocity);
        }
        else if (Mathf.Abs(_verticalVelocityChange) > 0.001f)
        {
            _locomotionMotor.AddVerticalVelocity(_verticalVelocityChange);
        }

        if (_hasFallSpeedLimitRequest)
        {
            _locomotionMotor.RequestFallSpeedLimit(_requestedFallSpeedLimit);
        }

        if (_requestedHorizontalSpeedMultiplier > 1f)
        {
            _locomotionMotor.RequestHorizontalSpeedMultiplier(
                _requestedHorizontalSpeedMultiplier
            );
        }
    }

    public void StopAll()
    {
        BeginFrame();

        if (_locomotionMotor != null)
        {
            _locomotionMotor.SetHorizontalVelocity(Vector3.zero);
        }
    }
}
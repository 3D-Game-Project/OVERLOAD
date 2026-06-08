using UnityEngine;

public class CoreMovementController : MonoBehaviour
{
    [Header("Core Horizontal Movement")]
    [SerializeField] private float _groundMoveSpeed = 1.5f;
    [SerializeField] private float _airMoveSpeed = 2.5f;
    [SerializeField] private float _acceleration = 6f;
    [SerializeField] private float _decceleration = 8f;

    [Header("Core Vertical Thruster")]
    [SerializeField] private float _verticalAcceleration = 45f;
    [SerializeField] private float _maxRiseSpeed = 5f;
    [SerializeField] private float _verticalEnergyCostPerSec = 12f;

    [Header("Core Dash")]
    [SerializeField] private float _coreDashSpeed = 12f;
    [SerializeField] private float _coreDashDuration = 0.15f;
    [SerializeField] private float _coreDashEnergyCost = 18f;

    [Header("Height Limit")]
    [SerializeField] private GroundSensor _groundSensor;
    [SerializeField] private float _maxHeightFromGround = 10f;
    [SerializeField] private float _heightSlowdownRange = 0.7f;
    [SerializeField] private float _ceilingDamping = 20f;

    [Header("References")]
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private PlayerLocomotionMotor _locomotionMotor;
    [SerializeField] private MovementCoordinator _movementCoordinator;
    [SerializeField] private CoreEnergyController _energyController;

    private Vector3 _currentHorizontalVelocity;

    private bool _hasGroundBelow;
    private float _heightFromGround;

    private bool _isCoreDashing;
    private float _coreDashTimer;
    private Vector3 _coreDashDirection;

    private void Awake()
    {
        if (_inputHandler == null)
            _inputHandler = GetComponent<PlayerInputHandler>();

        if (_locomotionMotor == null)
            _locomotionMotor = GetComponent<PlayerLocomotionMotor>();

        if (_movementCoordinator == null)
            _movementCoordinator = GetComponent<MovementCoordinator>();

        if (_energyController == null)
            _energyController = GetComponent<CoreEnergyController>();

        if (_groundSensor == null)
            _groundSensor = GetComponent<GroundSensor>();
    }

    public void HandleCoreMovement(
        LocomotionCommand command,
        bool hasLocomotionPart,
        bool hasBoosterPart,
        float deltaTime)
    {
        bool hasNoParts =
            !hasLocomotionPart && !hasBoosterPart;

        bool hasOnlyLeg =
            hasLocomotionPart && !hasBoosterPart;

        bool hasOnlyBooster =
            !hasLocomotionPart && hasBoosterPart;

        bool hasLegAndBooster =
            hasLocomotionPart && hasBoosterPart;

        if (hasNoParts)
        {
            HandleCoreHorizontalMovement(command, deltaTime);
            HandleCoreVerticalThruster(deltaTime);
            return;
        }

        if (hasOnlyLeg)
        {
            StopCoreHorizontalMovement();
            HandleCoreShortDash(command, deltaTime);
            return;
        }

        if (hasOnlyBooster)
        {
            HandleCoreHorizontalMovement(command, deltaTime);
            return;
        }

        if (hasLegAndBooster)
        {
            StopCoreHorizontalMovement();
            return;
        }
    }

    private void HandleCoreHorizontalMovement(
        LocomotionCommand command,
        float deltaTime)
    {
        if (_movementCoordinator == null)
            return;

        float moveSpeed = GetMoveSpeed();

        Vector3 targetVelocity = Vector3.zero;

        if (command.HasMoveInput)
        {
            targetVelocity = command.MoveDirection * moveSpeed;
        }

        float moveRate = command.HasMoveInput
            ? _acceleration
            : _decceleration;

        _currentHorizontalVelocity = Vector3.MoveTowards(
            _currentHorizontalVelocity,
            targetVelocity,
            moveRate * deltaTime
        );

        _movementCoordinator.SubmitGroundHorizontalVelocity(
            _currentHorizontalVelocity
        );
    }

    private void HandleCoreVerticalThruster(float deltaTime)
    {
        if (_movementCoordinator == null)
            return;

        if (_inputHandler == null)
            return;

        if (_locomotionMotor == null)
            return;

        if (!_inputHandler.IsJumpHeld)
            return;

        UpdateGroundInfo();

        if (!_hasGroundBelow)
            return;

        float heightFactor = GetHeightLimitFactor();

        if (heightFactor <= 0f)
        {
            DampenRiseSpeed(deltaTime);
            return;
        }

        if (!TryUseEnergyPerSec(_verticalEnergyCostPerSec))
            return;

        _locomotionMotor.IgnoreGroundSnapThisFrame();

        float acceleration = _verticalAcceleration * heightFactor;

        if (_locomotionMotor.VerticalVelocity < _maxRiseSpeed)
        {
            _movementCoordinator.SubmitVerticalAcceleration(
                acceleration,
                deltaTime
            );
        }

        float allowedRiseSpeed = _maxRiseSpeed * heightFactor;

        if (_locomotionMotor.VerticalVelocity > allowedRiseSpeed)
        {
            float targetVelocity = Mathf.MoveTowards(
                _locomotionMotor.VerticalVelocity,
                allowedRiseSpeed,
                _ceilingDamping * deltaTime
            );

            float velocityChange =
                targetVelocity - _locomotionMotor.VerticalVelocity;

            _movementCoordinator.SubmitVerticalVelocityChange(
                velocityChange
            );
        }
    }

    private void HandleCoreShortDash(LocomotionCommand command, float deltaTime)
    {
        if (_movementCoordinator == null)
            return;

        if (_inputHandler == null)
            return;

        // 대시 중이면 지속시간 동안 매 프레임 속도 전달
        if (_isCoreDashing)
        {
            _coreDashTimer -= deltaTime;

            if (_coreDashTimer > 0f)
            {
                Vector3 dashVelocity =
                    _coreDashDirection * _coreDashSpeed;

                _movementCoordinator.SubmitAdditionalHorizontalVelocity(
                    dashVelocity
                );

                return;
            }

            _isCoreDashing = false;
            _coreDashDirection = Vector3.zero;
        }

        // 새 대시 시작
        if (!_inputHandler.IsBoostPressed)
            return;

        Vector3 dashDirection = command.MoveDirection;

        // 입력이 없으면 카메라/코어가 바라보는 방향으로 대시
        if (dashDirection.sqrMagnitude < 0.001f)
        {
            dashDirection = command.LookDirection;
        }

        dashDirection.y = 0f;

        if (dashDirection.sqrMagnitude < 0.001f)
            return;

        if (!TryUseEnergy(_coreDashEnergyCost))
            return;

        _isCoreDashing = true;
        _coreDashTimer = _coreDashDuration;
        _coreDashDirection = dashDirection.normalized;
    }

    private void StopCoreHorizontalMovement()
    {
        _currentHorizontalVelocity = Vector3.zero;
    }

    private void UpdateGroundInfo()
    {
        _hasGroundBelow = false;
        _heightFromGround = Mathf.Infinity;

        if (_groundSensor != null &&
            _groundSensor.TryGetGround(out GroundInfo groundInfo))
        {
            _hasGroundBelow = true;
            _heightFromGround = groundInfo.Distance;
            return;
        }

        if (_locomotionMotor != null && _locomotionMotor.IsGrounded)
        {
            _hasGroundBelow = true;
            _heightFromGround = 0f;
        }
    }

    private float GetHeightLimitFactor()
    {
        if (!_hasGroundBelow)
            return 0f;

        float remainingHeight =
            _maxHeightFromGround - _heightFromGround;

        if (remainingHeight <= 0f)
            return 0f;

        if (_heightSlowdownRange <= 0f)
            return 1f;

        float factor = Mathf.Clamp01(
            remainingHeight / _heightSlowdownRange
        );

        return factor * factor;
    }

    private void DampenRiseSpeed(float deltaTime)
    {
        if (_locomotionMotor == null)
            return;

        if (_locomotionMotor.VerticalVelocity <= 0f)
            return;

        float targetVelocity = Mathf.MoveTowards(
            _locomotionMotor.VerticalVelocity,
            0f,
            _ceilingDamping * deltaTime
        );

        float velocityChange =
            targetVelocity - _locomotionMotor.VerticalVelocity;

        _movementCoordinator.SubmitVerticalVelocityChange(
            velocityChange
        );
    }

    private bool TryUseEnergy(float amount)
    {
        if (_energyController == null)
            return true;

        return _energyController.TryUseEnergy(amount);
    }

    private bool TryUseEnergyPerSec(float costPerSec)
    {
        if (_energyController == null)
            return true;

        return _energyController.TryUseEnergyPerSec(costPerSec);
    }

    private float GetMoveSpeed()
    {
        if (_locomotionMotor == null)
            return _groundMoveSpeed;

        return _locomotionMotor.IsGrounded
            ? _groundMoveSpeed
            : _airMoveSpeed;
    }

    public void StopCoreMovement()
    {
        _currentHorizontalVelocity = Vector3.zero;
        _isCoreDashing = false;
        _coreDashTimer = 0f;
        _coreDashDirection = Vector3.zero;
    }
}
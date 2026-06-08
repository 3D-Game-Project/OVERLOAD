using UnityEngine;

public class CoreMovementController : MonoBehaviour
{
    [Header("Core Horizontal Movement")]
    [SerializeField] private float _groundMoveSpeed = 1.5f;
    [SerializeField] private float _airMoveSpeed = 2.5f;
    [SerializeField] private float _acceleration = 6f;
    [SerializeField] private float _decceleration = 8f;

    [Header("Core Vertical Thruster")]
    [SerializeField] private float _verticalAcceleration = 18f;
    [SerializeField] private float _maxRiseSpeed = 3.5f;
    [SerializeField] private float _verticalEnergyCostPerSec = 12f;

    [Header("Height Limit")]
    [SerializeField] private GroundSensor _groundSensor;
    [SerializeField] private float _maxHeightFromGround = 2.5f;
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
        HandleCoreHorizontalMovement(
            command,
            hasLocomotionPart,
            deltaTime
        );

        HandleCoreVerticalThruster(
            hasBoosterPart,
            deltaTime
        );
    }

    private void HandleCoreHorizontalMovement(
        LocomotionCommand command,
        bool hasLocomotionPart,
        float deltaTime)
    {
        if (_movementCoordinator == null)
            return;

        // 다리 파츠가 있으면 일반 수평 이동은 다리 파츠가 담당
        if (hasLocomotionPart)
        {
            _currentHorizontalVelocity = Vector3.zero;
            return;
        }

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

    private void HandleCoreVerticalThruster(
        bool hasBoosterPart,
        float deltaTime)
    {
        if (_movementCoordinator == null)
            return;

        if (_inputHandler == null)
            return;

        if (_locomotionMotor == null)
            return;

        // 부스터 파츠가 있으면 수직 이동은 부스터가 담당
        if (hasBoosterPart)
            return;

        UpdateGroundInfo();

        if (!_inputHandler.IsJumpHeld)
            return;

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
    }
}
using UnityEngine;

public class PlayerCoreLocomotion : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerLocomotionMotor _motor;
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private GroundSensor _groundSensor;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _visualRoot;

    [Header("Movement")]
    [SerializeField] private float _groundMoveSpeed = 1.5f;
    [SerializeField] private float _airMoveSpeed = 2.5f;
    [SerializeField] private float _acceleration = 6f;
    [SerializeField] private float _decceleration = 8f;
    [SerializeField] private float _inputDeadZone = 0.05f;

    [Header("Rotation")]
    [SerializeField] private float _moveRotationSpeed = 6f;

    [Header("Booster Output")]
    [SerializeField] private float _maxOutput = 100f;
    [SerializeField] private float _outputDrainPerSec = 35f;
    [SerializeField] private float _outputRecoverPerSec = 45f;
    [SerializeField] private float _outputEmptyCooldown = 0.5f;
    [SerializeField] private float _minOutputToUse = 0.1f;

    [Header("Booster Physics")]
    [SerializeField] private float _boosterAcceleration = 35f;
    [SerializeField] private float _maxRiseSpeed = 5f;
    [SerializeField] private float _maxHeightFromGround = 3f;
    [SerializeField] private float _heightSlowdownRange = 0.7f;
    [SerializeField] private float _ceilingDamping = 20f;

    [Header("Visual")]
    [SerializeField] private float _tiltAmount = 8f;
    [SerializeField] private float _tiltSmooth = 8f;

    private float _currentOutput;
    private float _outputCooldownTimer;

    private Vector3 _currentHorizontalVelocity;

    private bool _isBoosterActive;
    private bool _isOutputLocked;

    private bool _hasGroundBelow;
    private float _heightFromGround;

    public float CurrentOutput => _currentOutput;
    public float MaxOutput => _maxOutput;
    public float OutputRatio => _maxOutput <= 0f ? 0f : _currentOutput / _maxOutput;

    public bool IsBoosterActive => _isBoosterActive;
    public bool IsThrusterActive => _isBoosterActive;
    public bool IsOutputLocked => _isOutputLocked;
    public float HeightFromGround => _heightFromGround;

    private void Awake()
    {
        if (_motor == null)
            _motor = GetComponent<PlayerLocomotionMotor>();

        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInputHandler>();

        if (_groundSensor == null)
            _groundSensor = GetComponent<GroundSensor>();

        if (_cameraTransform == null && Camera.main != null)
            _cameraTransform = Camera.main.transform;

        _currentOutput = _maxOutput;
    }

    private void Update()
    {
        Vector2 moveInput = ReadMoveInput();

        UpdateGroundInfo();

        bool hasMoveInput = moveInput.sqrMagnitude > 0.001f;

        UpdateBoosterState();
        UpdateOutput();

        UpdateCoreRotation(hasMoveInput);
        UpdateHorizontalMovement(moveInput, hasMoveInput);
        UpdateVerticalBooster();

        UpdateVisualTilt(moveInput, hasMoveInput);
    }

    private Vector2 ReadMoveInput()
    {
        if (_playerInput == null)
            return Vector2.zero;

        Vector2 input = _playerInput.MoveInput;

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        if (Mathf.Abs(input.x) < _inputDeadZone)
            input.x = 0f;

        if (Mathf.Abs(input.y) < _inputDeadZone)
            input.y = 0f;

        return input;
    }

    private void UpdateGroundInfo()
    {
        _hasGroundBelow = false;
        _heightFromGround = Mathf.Infinity;

        if (_groundSensor != null && _groundSensor.TryGetGround(out GroundInfo groundInfo))
        {
            _hasGroundBelow = true;
            _heightFromGround = groundInfo.Distance;
            return;
        }

        // 센서가 잠깐 바닥을 못 잡아도 CharacterController가 grounded면 바닥으로 인정
        if (_motor != null && _motor.IsGrounded)
        {
            _hasGroundBelow = true;
            _heightFromGround = 0f;
        }
    }

    private void UpdateBoosterState()
    {
        if (_playerInput == null)
        {
            _isBoosterActive = false;
            return;
        }

        bool hasEnoughOutput = _currentOutput > _minOutputToUse;
        bool isBelowMaxHeight = _hasGroundBelow && _heightFromGround < _maxHeightFromGround;

        _isBoosterActive =
            _playerInput.IsJumpPressed &&
            !_isOutputLocked &&
            hasEnoughOutput &&
            isBelowMaxHeight;
    }

    private void UpdateOutput()
    {
        if (_isOutputLocked)
        {
            _isBoosterActive = false;

            if (_outputCooldownTimer > 0f)
            {
                _outputCooldownTimer -= Time.deltaTime;
                return;
            }

            _isOutputLocked = false;
        }

        if (_isBoosterActive)
        {
            _currentOutput -= _outputDrainPerSec * Time.deltaTime;

            if (_currentOutput <= 0f)
            {
                _currentOutput = 0f;
                _isBoosterActive = false;
                _isOutputLocked = true;
                _outputCooldownTimer = _outputEmptyCooldown;
            }

            return;
        }

        if (_currentOutput < _maxOutput)
        {
            _currentOutput += _outputRecoverPerSec * Time.deltaTime;
            _currentOutput = Mathf.Min(_currentOutput, _maxOutput);
        }
    }

    private void UpdateCoreRotation(bool hasMoveInput)
    {
        // 마우스만 돌릴 때는 코어가 회전하지 않음
        // WASD 입력이 있을 때만 카메라 방향으로 천천히 회전
        if (!hasMoveInput)
            return;

        Vector3 cameraForward = GetCameraForwardOnPlane();

        if (cameraForward.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(cameraForward, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            _moveRotationSpeed * Time.deltaTime
        );
    }

    private Vector3 GetCameraForwardOnPlane()
    {
        if (_cameraTransform == null)
            return transform.forward;

        Vector3 forward = _cameraTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return transform.forward;

        return forward.normalized;
    }

    private void UpdateHorizontalMovement(Vector2 moveInput, bool hasMoveInput)
    {
        float moveSpeed = _motor.IsGrounded
            ? _groundMoveSpeed
            : _airMoveSpeed;

        Vector3 targetVelocity = Vector3.zero;

        if (hasMoveInput)
        {
            // 핵심:
            // WASD 이동은 코어 자신의 로컬 축 기준
            // W/S = forward/back
            // A/D = left/right
            Vector3 moveDirection =
                transform.forward * moveInput.y +
                transform.right * moveInput.x;

            moveDirection.y = 0f;

            if (moveDirection.sqrMagnitude > 1f)
                moveDirection.Normalize();

            targetVelocity = moveDirection * moveSpeed;
        }

        float moveRate = hasMoveInput
            ? _acceleration
            : _decceleration;

        _currentHorizontalVelocity = Vector3.MoveTowards(
            _currentHorizontalVelocity,
            targetVelocity,
            moveRate * Time.deltaTime
        );

        _motor.SetHorizontalVelocity(_currentHorizontalVelocity);
    }

    private void UpdateVerticalBooster()
    {
        if (!_hasGroundBelow)
            return;

        float heightFactor = GetHeightLimitFactor();

        // 최대 높이에 도달했거나 거의 도달한 상태
        if (heightFactor <= 0f)
        {
            // 위로 올라가는 속도만 부드럽게 줄임
            if (_motor.VerticalVelocity > 0f)
            {
                float dampedVelocity = Mathf.MoveTowards(
                    _motor.VerticalVelocity,
                    0f,
                    _ceilingDamping * Time.deltaTime
                );

                _motor.SetVerticalVelocity(dampedVelocity);
            }

            return;
        }

        if (!_isBoosterActive)
            return;

        _motor.IgnoreGroundSnapThisFrame();

        // 높이에 가까워질수록 부스터 가속도 감소
        float boosterForce = _boosterAcceleration * heightFactor;

        _motor.AddVerticalVelocity(boosterForce * Time.deltaTime);

        // 높이에 가까워질수록 허용 상승 속도도 감소
        float allowedRiseSpeed = _maxRiseSpeed * heightFactor;

        if (_motor.VerticalVelocity > allowedRiseSpeed)
        {
            float dampedVelocity = Mathf.MoveTowards(
                _motor.VerticalVelocity,
                allowedRiseSpeed,
                _ceilingDamping * Time.deltaTime
            );

            _motor.SetVerticalVelocity(dampedVelocity);
        }
    }

    private float GetHeightLimitFactor()
    {
        if (!_hasGroundBelow)
            return 0f;

        float remainingHeight = _maxHeightFromGround - _heightFromGround;

        if (remainingHeight <= 0f)
            return 0f;

        if (_heightSlowdownRange <= 0f)
            return 1f;

        float factor = Mathf.Clamp01(remainingHeight / _heightSlowdownRange);

        // 부드러운 감속 곡선
        return factor * factor;
    }

    private void UpdateVisualTilt(Vector2 moveInput, bool hasMoveInput)
    {
        if (_visualRoot == null)
            return;

        float targetPitch = hasMoveInput ? moveInput.y * _tiltAmount : 0f;
        float targetRoll = hasMoveInput ? -moveInput.x * _tiltAmount : 0f;

        Quaternion targetRotation = Quaternion.Euler(
            targetPitch,
            0f,
            targetRoll
        );

        _visualRoot.localRotation = Quaternion.Slerp(
            _visualRoot.localRotation,
            targetRotation,
            _tiltSmooth * Time.deltaTime
        );
    }

    public void SetGroundSensor(GroundSensor newGroundSensor)
    {
        _groundSensor = newGroundSensor;
    }
}
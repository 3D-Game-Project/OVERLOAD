using UnityEngine;

public class PlayerCoreLocomotion : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerLocomotionMotor _motor;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private PlayerInputHandler _playerInput;

    [Header("Core Movement Settings")]
    [SerializeField] private float _maxMoveSpeed = 2.5f;
    [SerializeField] private float _acceleration = 5f;
    [SerializeField] private float _decceleration = 8f;
    [SerializeField] private float _rotationSpeed = 8f;

    [Header("Thruster Settings")]
    [SerializeField] private float _maxOutput = 100f;
    [SerializeField] private float _outputDrainPerSec = 20f;
    [SerializeField] private float _outputRecoverPerSec = 35f;
    [SerializeField] private float _minOutputToUse = 0.1f;

    [Header("Hover Settings")]
    [SerializeField] private float _hoverHeight = 2f;
    [SerializeField] private float _hoverForce = 20f;
    [SerializeField] private float _maxRiseSpeed = 5f;
    [SerializeField] private float _groundCheckDistance = 3f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Visual")]
    [SerializeField] private float _tiltAmount = 8f;
    [SerializeField] private float _tiltSmooth = 8f;

    private float _currentOutput;
    private Vector3 _currentHorizontalVeocity;
    private bool _hasMoveInput;
    private bool _isThrusterActive;
    private float _distanceToGround;

    public float CurrentOutput => _currentOutput;
    public float MaxOutput => _maxOutput;
    public bool IsThrusterActive => _isThrusterActive;

    private void Awake()
    {
        if (_motor == null)
            _motor = GetComponent<PlayerLocomotionMotor>();

        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInputHandler>();

        if (_cameraTransform == null && Camera.main != null)
            _cameraTransform = Camera.main.transform;

        _currentOutput = _maxOutput;
    }

    private void Update()
    {
        Vector2 moveInput = ReadMoveInput();

        CheckGroundDistance();
        UpdateThrusterState(moveInput);
        UpdateOutput();
        UpdateHorizontalMovement(moveInput);
        UpdateHover();
        UpdateRotation(moveInput);
        UpdateVisualTilt(moveInput);
    }

    private Vector2 ReadMoveInput()
    {
        if (_playerInput == null)
        {
            _hasMoveInput = false;
            return Vector2.zero;
        }

        Vector2 input = _playerInput.MoveInput;

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        _hasMoveInput = input.sqrMagnitude > 0.01f;

        return input;
    }

    private void CheckGroundDistance()
    {
        Vector3 origin = transform.position + Vector3.up * 0.2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _groundCheckDistance, _groundLayer))
        {
            _distanceToGround = hit.distance;
        }
        else
        {
            _distanceToGround = Mathf.Infinity;
        }
    }

    private void UpdateThrusterState(Vector2 moveInput)
    {
        bool hasEnoughOutput = _currentOutput > _minOutputToUse;

        _isThrusterActive = _hasMoveInput && hasEnoughOutput;
    }

    private void UpdateOutput()
    {
        if (_isThrusterActive)
        {
            _currentOutput -= _outputDrainPerSec * Time.deltaTime;
        }
        else if (_motor.IsGrounded)
        {
            _currentOutput += _outputRecoverPerSec * Time.deltaTime;
        }
    }

    private void UpdateHorizontalMovement(Vector2 moveInput)
    {
        Vector3 moveDirection = GetCameraBasedMoveDirection(moveInput);

        Vector3 targetVelocity = Vector3.zero;

        if (_isThrusterActive)
        {
            targetVelocity = moveDirection * _maxMoveSpeed;
        }

        float moveRate = _isThrusterActive ? _acceleration : _decceleration;

        _currentHorizontalVeocity = Vector3.MoveTowards(
            _currentHorizontalVeocity,
            targetVelocity,
            moveRate * Time.deltaTime
        );

        _motor.SetHorizontalVelocity(_currentHorizontalVeocity);
    }

    private void UpdateHover()
    {
        if (!_isThrusterActive)
            return;

        if (_distanceToGround == Mathf.Infinity)
            return;

        float hoverEror = _hoverHeight - _distanceToGround;
        float verticalVelocity = hoverEror * _hoverForce;

        verticalVelocity = Mathf.Clamp(verticalVelocity, -1.5f, _maxRiseSpeed);

        _motor.SetVerticalVelocity(verticalVelocity);
    }

    private Vector3 GetCameraBasedMoveDirection(Vector2 input)
    {
        if (_cameraTransform == null)
        {
            return new Vector3(input.x, 0f, input.y);
        }

        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * input.y + right * input.x;

        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        return direction;
    }

    private void UpdateRotation(Vector2 moveInput)
    {
        if (!_isThrusterActive)
            return;

        Vector3 moveDirection = GetCameraBasedMoveDirection(moveInput);

        if (moveDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            _rotationSpeed * Time.deltaTime);
    }

    private void UpdateVisualTilt(Vector2 moveInput)
    {
        if (_visualRoot == null)
            return;

        float targetPitch = moveInput.y * _tiltAmount;
        float targetRoll = -moveInput.x * _tiltAmount;

        if (!_isThrusterActive)
        {
            targetPitch = 0f;
            targetRoll = 0f;
        }

        Quaternion targetRotatino = Quaternion.Euler(targetPitch, 0f, targetRoll);

        _visualRoot.localRotation = Quaternion.Slerp(
            _visualRoot.localRotation,
            targetRotatino,
            _tiltSmooth * Time.deltaTime
        );
    }
}

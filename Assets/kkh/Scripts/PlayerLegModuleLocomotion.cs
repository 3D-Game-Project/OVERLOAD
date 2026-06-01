using UnityEngine;

public class PlayerLegModuleLocomotion : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerLocomotionMotor _motor;
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private Transform _cameraTransform;

    [Header("Rotation Roots")]
    [SerializeField] private Transform _coreYawRoot;
    [SerializeField] private Transform _legYawRoot;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 3.5f;
    [SerializeField] private float _acceleration = 12f;
    [SerializeField] private float _decceleration = 16f;
    [SerializeField] private float _inputDeadZone = 0.05f;

    [Header("Air Control")]
    [SerializeField] private bool _canMoveInAir = false;
    [SerializeField] private float _airControlMultiplier = 0.2f;

    [Header("Core Rotation")]
    [SerializeField] private float _coreYawFollowSpeed = 14f;

    [Header("Leg Rotation")]
    [SerializeField] private float _legTurnSpeed = 5f;

    private Vector3 _currentHorizontalVelocity;

    private void Awake()
    {
        if (_motor == null)
            _motor = GetComponentInParent<PlayerLocomotionMotor>();

        if (_playerInput == null)
            _playerInput = GetComponentInParent<PlayerInputHandler>();

        if (_cameraTransform == null && Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (_motor == null || _playerInput == null)
            return;

        Vector2 moveInput = ReadMoveInput();

        Vector3 cameraForward = GetCameraForwardOnPlane();
        Vector3 cameraRight = GetCameraRightOnPlane();

        Vector3 moveDirection = GetCameraBasedMoveDirection(
            moveInput,
            cameraForward,
            cameraRight
        );

        bool hasMoveInput = moveDirection.sqrMagnitude > 0.001f;

        UpdateCoreYaw(cameraForward);
        UpdateLegYaw(moveDirection, hasMoveInput);
        UpdateMovement(moveDirection, hasMoveInput);
    }

    private Vector2 ReadMoveInput()
    {
        Vector2 input = _playerInput.MoveInput;

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        if (Mathf.Abs(input.x) < _inputDeadZone)
            input.x = 0f;

        if (Mathf.Abs(input.y) < _inputDeadZone)
            input.y = 0f;

        return input;
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

    private Vector3 GetCameraRightOnPlane()
    {
        if (_cameraTransform == null)
            return transform.right;

        Vector3 right = _cameraTransform.right;
        right.y = 0f;

        if (right.sqrMagnitude < 0.001f)
            return transform.right;

        return right.normalized;
    }

    private Vector3 GetCameraBasedMoveDirection(
        Vector2 moveInput,
        Vector3 cameraForward,
        Vector3 cameraRight
    )
    {
        Vector3 direction =
            cameraForward * moveInput.y +
            cameraRight * moveInput.x;

        direction.y = 0f;

        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        return direction;
    }

    private void UpdateCoreYaw(Vector3 cameraForward)
    {
        if (_coreYawRoot == null)
            return;

        if (cameraForward.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(cameraForward, Vector3.up);

        _coreYawRoot.rotation = Quaternion.Slerp(
            _coreYawRoot.rotation,
            targetRotation,
            _coreYawFollowSpeed * Time.deltaTime
        );
    }

    private void UpdateLegYaw(Vector3 moveDirection, bool hasMoveInput)
    {
        if (_legYawRoot == null)
            return;

        // 정지 상태에서는 다리가 회전하지 않음
        if (!hasMoveInput)
            return;

        if (moveDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        _legYawRoot.rotation = Quaternion.Slerp(
            _legYawRoot.rotation,
            targetRotation,
            _legTurnSpeed * Time.deltaTime
        );
    }

    private void UpdateMovement(Vector3 moveDirection, bool hasMoveInput)
    {
        Vector3 targetVelocity = Vector3.zero;

        if (hasMoveInput)
        {
            float controlMultiplier = GetControlMultiplier();
            targetVelocity = moveDirection * _moveSpeed * controlMultiplier;
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

    private float GetControlMultiplier()
    {
        if (_motor.IsGrounded)
            return 1f;

        if (_canMoveInAir)
            return _airControlMultiplier;

        return 0f;
    }

    private void OnDisable()
    {
        if (_motor != null)
            _motor.SetHorizontalVelocity(Vector3.zero);

        _currentHorizontalVelocity = Vector3.zero;
    }
}
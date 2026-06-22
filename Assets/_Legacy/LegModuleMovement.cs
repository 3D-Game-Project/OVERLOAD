using UnityEngine;

public class LegModuleMovement : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerLocomotionMotor _motor;
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private Transform _cameraTransform;

    [Header("Rotation Roots")]
    [SerializeField] private Transform _coreYawRoot;
    [SerializeField] private Transform _legYawRoot;

    // 기본적인 수평 이동 설정
    // 속도 및 가감속, 데드존 설정
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 3.5f;
    [SerializeField] private float _acceleration = 12f;
    [SerializeField] private float _decceleration = 16f;
    [SerializeField] private float _inputDeadZone = 0.05f;

    // 다리 모듈 회전용
    // 코어와 독립적으로 작동을 위해 변수 두개로 나눔
    [Header("Rotation Settings")]
    [SerializeField] private float _coreYawSpeed = 720f;
    [SerializeField] private float _legYawSpeed = 360f;

    [Header("Model Offset")]
    [SerializeField] private float _coreForwardYawOffset = 0f;
    [SerializeField] private float _legForwardYawOffset = 0f;

    // 공중에 뜨면 이동이 속도 감소를 위해 추가
    // 부스터로 기능 이동할수도
    [Header("Air Control")]
    [SerializeField] private bool _groundMoveOnly = true;
    [SerializeField] private float _airControlMultiplier = 0.2f;

    private Vector3 _currentHorizontalVelocity;

    private void Awake()
    {
        if (_legYawRoot == null)
            _legYawRoot = transform;

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
        UpdateLegYaw(cameraForward, hasMoveInput);
        UpdateMovement(moveDirection, hasMoveInput);
    }

    // 코어와 동일하게 input 읽어오기
    // Update에서 일괄 적용
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

    // 코어와 동일한 카메라를 통해 방향 확인하는 함수
    // 이후에 에임 용으로 따로 뺄 예정
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

    // 위와 동일
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

        Quaternion targetRotation =
            Quaternion.LookRotation(cameraForward, Vector3.up) *
            Quaternion.Euler(0f, _coreForwardYawOffset, 0f);

        _coreYawRoot.rotation = Quaternion.RotateTowards(
            _coreYawRoot.rotation,
            targetRotation,
            _coreYawSpeed * Time.deltaTime
        );
    }

    private void UpdateLegYaw(Vector3 moveDirection, bool hasMoveInput)
    {
        if (_legYawRoot == null)
            return;

        // 마우스만 돌릴 때는 다리 회전 안 함
        if (!hasMoveInput)
            return;

        if (moveDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(moveDirection, Vector3.up) *
            Quaternion.Euler(0f, _legForwardYawOffset, 0f);

        _legYawRoot.rotation = Quaternion.RotateTowards(
            _legYawRoot.rotation,
            targetRotation,
            _legYawSpeed * Time.deltaTime
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

        if (_groundMoveOnly)
            return 0f;

        return _airControlMultiplier;
    }

    private void OnDisable()
    {
        _currentHorizontalVelocity = Vector3.zero;

        if (_motor != null)
            _motor.SetHorizontalVelocity(Vector3.zero);
    }
}
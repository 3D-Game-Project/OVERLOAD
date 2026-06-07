using UnityEngine;

public class LegPartController : MonoBehaviour
{
    [Header("Base Rotation")]
    [SerializeField] private float _baseCoreYawSpeed = 720f;
    [SerializeField] private float _baseLegYawSpeed = 360f;

    [Header("Input")]
    [SerializeField] private float _inputDeadZone = 0.05f;

    [Header("Model Offset")]
    [SerializeField] private float _coreForwardYawOffset = 0f;
    [SerializeField] private float _legForwardYawOffset = 0f;

    private LegPartsData _data;
    private LegTypeProfile _profile;
    private CorePartContext _context;

    private Vector3 _currentHorizontalVelocity;

    public LegPartsData Data => _data;
    public LegTypeProfile Profile => _profile;

    public void Initialize(LegPartsData data, CorePartContext context)
    {
        _data = data;
        _context = context;

        if (_data == null)
        {
            Debug.LogError("LegPartsData가 없습니다.");
            return;
        }

        _profile = _data.LegTypeProfile;

        if (_profile == null)
        {
            Debug.LogError($"{_data.PartsName}에 LegTypeProfile이 없습니다.");
            return;
        }

        Debug.Log(
            $"다리 파츠 초기화 완료\n" +
            $"Parts Name: {_data.PartsName}\n" +
            $"Leg Type: {_data.LegType}\n" +
            $"Move Speed: {_data.MoveSpeed}"
        );
    }

    private void Update()
    {
        if (_data == null || _profile == null || _context == null)
            return;

        if (_context.InputHandler == null || _context.LocomotionMotor == null)
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
        Vector2 input = _context.InputHandler.MoveInput;

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
        if (_context.CameraTransform == null)
            return transform.forward;

        Vector3 forward = _context.CameraTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return transform.forward;

        return forward.normalized;
    }

    private Vector3 GetCameraRightOnPlane()
    {
        if (_context.CameraTransform == null)
            return transform.right;

        Vector3 right = _context.CameraTransform.right;
        right.y = 0f;

        if (right.sqrMagnitude < 0.001f)
            return transform.right;

        return right.normalized;
    }

    private Vector3 GetCameraBasedMoveDirection(
        Vector2 moveInput,
        Vector3 cameraForward,
        Vector3 cameraRight)
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
        if (_context.CoreYawRoot == null)
            return;

        if (cameraForward.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(cameraForward, Vector3.up) *
            Quaternion.Euler(0f, _coreForwardYawOffset, 0f);

        _context.CoreYawRoot.rotation = Quaternion.RotateTowards(
            _context.CoreYawRoot.rotation,
            targetRotation,
            _baseCoreYawSpeed * Time.deltaTime
        );
    }

    private void UpdateLegYaw(Vector3 moveDirection, bool hasMoveInput)
    {
        if (_context.LegYawRoot == null)
            return;

        if (!hasMoveInput)
            return;

        if (moveDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(moveDirection, Vector3.up) *
            Quaternion.Euler(0f, _legForwardYawOffset, 0f);

        float finalLegYawSpeed =
            _baseLegYawSpeed * _profile.TurnSpeedMultiplier;

        _context.LegYawRoot.rotation = Quaternion.RotateTowards(
            _context.LegYawRoot.rotation,
            targetRotation,
            finalLegYawSpeed * Time.deltaTime
        );
    }

    private void UpdateMovement(Vector3 moveDirection, bool hasMoveInput)
    {
        Vector3 targetVelocity = Vector3.zero;

        if (hasMoveInput)
        {
            targetVelocity = moveDirection * _data.MoveSpeed;
        }

        float moveRate = hasMoveInput
            ? _data.Acceleration
            : _data.Decceleration;

        _currentHorizontalVelocity = Vector3.MoveTowards(
            _currentHorizontalVelocity,
            targetVelocity,
            moveRate * Time.deltaTime
        );

        _context.LocomotionMotor.SetHorizontalVelocity(_currentHorizontalVelocity);
    }

    private void OnDisable()
    {
        _currentHorizontalVelocity = Vector3.zero;

        if (_context != null && _context.LocomotionMotor != null)
        {
            _context.LocomotionMotor.SetHorizontalVelocity(Vector3.zero);
        }
    }
}
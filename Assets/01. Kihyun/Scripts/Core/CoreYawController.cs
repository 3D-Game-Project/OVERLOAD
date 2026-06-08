using UnityEngine;

public class CoreYawController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _coreYawRoot;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private CorePartsController _corePartsController;

    [Header("Yaw Settings")]
    [SerializeField] private float _coreOnlyYawSpeed = 360f;
    [SerializeField] private float _withLegYawSpeed = 720f;
    [SerializeField] private float _yawOffset = 0f;

    [Header("Input")]
    [SerializeField] private float _inputDeadZone = 0.05f;

    private void Awake()
    {
        if (_inputHandler == null)
            _inputHandler = GetComponent<PlayerInputHandler>();

        if (_corePartsController == null)
            _corePartsController = GetComponent<CorePartsController>();

        if (_cameraTransform == null && Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (_coreYawRoot == null)
            return;

        if (_cameraTransform == null)
            return;

        bool hasLegModule =
            _corePartsController != null &&
            _corePartsController.HasLocomotionPart;

        bool hasMoveInput = HasMoveInput();

        // 핵심 조건
        // 다리 없음: 이동 입력이 있을 때만 회전
        // 다리 있음: 이동 입력이 없어도 카메라 방향으로 회전
        if (!hasLegModule && !hasMoveInput)
            return;

        Vector3 lookDirection = GetCameraForwardOnPlane();

        if (lookDirection.sqrMagnitude < 0.001f)
            return;

        float yawSpeed = hasLegModule
            ? _withLegYawSpeed
            : _coreOnlyYawSpeed;

        Quaternion targetRotation =
            Quaternion.LookRotation(lookDirection, Vector3.up) *
            Quaternion.Euler(0f, _yawOffset, 0f);

        _coreYawRoot.rotation = Quaternion.RotateTowards(
            _coreYawRoot.rotation,
            targetRotation,
            yawSpeed * Time.deltaTime
        );
    }

    private bool HasMoveInput()
    {
        if (_inputHandler == null)
            return false;

        Vector2 input = _inputHandler.MoveInput;

        if (Mathf.Abs(input.x) < _inputDeadZone)
            input.x = 0f;

        if (Mathf.Abs(input.y) < _inputDeadZone)
            input.y = 0f;

        return input.sqrMagnitude > 0.001f;
    }

    private Vector3 GetCameraForwardOnPlane()
    {
        Vector3 forward = _cameraTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return _coreYawRoot.forward;

        return forward.normalized;
    }
}
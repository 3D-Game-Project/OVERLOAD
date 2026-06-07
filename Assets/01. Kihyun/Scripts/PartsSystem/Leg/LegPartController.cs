using UnityEngine;

public class LegPartController : PartBehaviour, ILocomotionPart
{
    [Header("Base Rotation")]
    [SerializeField] private float _baseCoreYawSpeed = 720f;
    [SerializeField] private float _baseLegYawSpeed = 360f;

    [Header("Model Offset")]
    [SerializeField] private float _coreForwardYawOffset = 0f;
    [SerializeField] private float _legForwardYawOffset = 0f;

    private LegPartsData _legData;
    private LegTypeProfile _profile;

    private Vector3 _currentHorizontalVelocity;

    public override void Initialize(PartsData data, CorePartContext context)
    {
        base.Initialize(data, context);

        if (!TryGetData(out _legData))
        {
            Debug.LogError($"{gameObject.name}에는 LegPartsData가 필요합니다.");
            return;
        }

        _profile = _legData.LegTypeProfile;

        if (_profile == null)
        {
            Debug.LogError($"{_legData.PartsName}에 LegTypeProfile이 없습니다.");
            return;
        }

        Debug.Log(
            $"다리 파츠 초기화 완료\n" +
            $"Parts Name: {_legData.PartsName}\n" +
            $"Leg Type: {_legData.LegType}\n" +
            $"Move Speed: {_legData.MoveSpeed}"
        );
    }

    public void HandleLocomotion(LocomotionCommand command, float deltaTime)
    {
        if (!IsInitialized())
            return;

        if (_legData == null || _profile == null)
            return;

        if (_context.LocomotionMotor == null)
            return;

        UpdateCoreYaw(command.LookDirection, deltaTime);
        UpdateLegYaw(command.MoveDirection, command.HasMoveInput, deltaTime);
        UpdateMovement(command.MoveDirection, command.HasMoveInput, deltaTime);
    }

    public void StopLocomotion()
    {
        _currentHorizontalVelocity = Vector3.zero;

        if (_context != null && _context.MovementCoordinator != null)
        {
            _context.MovementCoordinator.StopAll();
        }
    }

    private void UpdateCoreYaw(Vector3 lookDirection, float deltaTime)
    {
        if (_context.CoreYawRoot == null)
            return;

        if (lookDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(lookDirection, Vector3.up) *
            Quaternion.Euler(0f, _coreForwardYawOffset, 0f);

        _context.CoreYawRoot.rotation = Quaternion.RotateTowards(
            _context.CoreYawRoot.rotation,
            targetRotation,
            _baseCoreYawSpeed * deltaTime
        );
    }

    private void UpdateLegYaw(Vector3 moveDirection, bool hasMoveInput, float deltaTime)
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
            finalLegYawSpeed * deltaTime
        );
    }

    private void UpdateMovement(Vector3 moveDirection, bool hasMoveInput, float deltaTime)
    {
        Vector3 targetVelocity = Vector3.zero;

        if (hasMoveInput)
        {
            targetVelocity = moveDirection * _legData.MoveSpeed;
        }

        float moveRate = hasMoveInput
            ? _legData.Acceleration
            : _legData.Decceleration;

        _currentHorizontalVelocity = Vector3.MoveTowards(
            _currentHorizontalVelocity,
            targetVelocity,
            moveRate * deltaTime
        );

        if (_context.MovementCoordinator != null)
        {
            _context.MovementCoordinator.SubmitGroundHorizontalVelocity(
                _currentHorizontalVelocity
            );
        }
    }

    public override void OnDetached()
    {
        StopLocomotion();
    }

    private void OnDisable()
    {
        StopLocomotion();
    }
}
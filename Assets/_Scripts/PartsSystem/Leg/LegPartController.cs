using UnityEngine;

public class LegPartController : PartBehaviour, ILocomotionPart
{
    [Header("Base Rotation")]
    [SerializeField] private float _baseLegYawSpeed = 360f;

    [Header("Model Offset")]
    [SerializeField] private float _legForwardYawOffset = 0f;

    private LegPartsData _legData;
    private LegTypeProfile _profile;

    [Header("Owner")]
    [SerializeField] private int _ownerLayer;

    private Vector3 _currentHorizontalVelocity;

    public override void Initialize(PartsData data, CorePartContext context)
    {
        base.Initialize(data, context);

        if (!TryGetData(out _legData))
        {
            Debug.LogError($"{gameObject.name}에는 LegPartsData가 필요합니다.");
            return;
        }

        if (_context != null && _context.OwnerRoot != null)
        {
            _ownerLayer = _context.OwnerRoot.gameObject.layer;
            gameObject.layer = _ownerLayer;
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

        if (TryGetComponent(out DurabilityController durability))
        {
            durability.InitializePart(_legData, false);
        }
    }

    public void HandleLocomotion(LocomotionCommand command, float deltaTime)
    {
        if (!IsInitialized())
            return;

        if (_legData == null || _profile == null)
            return;

        if (_context.MovementCoordinator == null)
            return;

        // 다리 모듈의 방향은 이동 방향이 아니라 카메라 Forward 기준
        UpdateLegYaw(command.LookDirection, command.HasMoveInput, deltaTime);

        // 실제 이동은 카메라 기준 WASD 이동 방향
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
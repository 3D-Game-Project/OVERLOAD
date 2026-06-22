using UnityEngine;

public class LegPartController : PartBehaviour, ILocomotionPart
{
    [Header("Base Rotation")]
    [SerializeField] private float _baseLegYawSpeed = 360f;

    [Header("Model Offset")]
    [SerializeField] private float _legForwardYawOffset = 0f;

    [Header("Animation")]
    [SerializeField] private LegAnimationDriver _animationDriver;

    private LegPartsData _legData;
    private LegTypeProfile _profile;

    [Header("Owner")]
    [SerializeField] private int _ownerLayer;

    [Header("Core Visual Follow")]
    [SerializeField] private Transform _coreFollowTarget;
    [SerializeField] private bool _useCoreVisualFollow = true;
    [SerializeField] private float _coreFollowWeight = 1f;
    [SerializeField] private float _coreFollowSmooth = 12f;
    [SerializeField] private float _coreFollowMaxOffset = 1f;

    [Header("Terrain")]
    [SerializeField] private LegTerrainAdapter _terrainAdapter;

    [Header("Car Steering")]
    [SerializeField] private float _baseCarYawSpeed = 90f;

    [Header("Buggy Wheel Visual")]
    [SerializeField]
    private BuggyWheelVisualController _buggyWheelVisualController;

    private Vector3 _currentHorizontalVelocity;
    private bool _isCameraFollowTurning;

    private float _currentTurnInput;
    private bool _isTurningThisFrame;

    private Vector3 _coreBaseLocalPosition;
    private float _coreFollowBaseLocalY;
    private bool _hasCoreFollowBase;
    private Vector3 _coreFollowBaseLocalPosition;

    private bool _isRunningByBooster;

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

        if (_animationDriver == null)
            _animationDriver = GetComponentInChildren<LegAnimationDriver>();

        if (_animationDriver != null)
            _animationDriver.Initialize(_legData.AnimatorController);
        else
            Debug.LogWarning($"{gameObject.name}에 LegAnimationDriver가 없습니다.");

        if (_terrainAdapter == null)
            _terrainAdapter = GetComponentInChildren<LegTerrainAdapter>();

        if (_terrainAdapter != null)
            _terrainAdapter.Initialize(_profile, _context.LocomotionMotor);

        if (_buggyWheelVisualController == null)
        {
            _buggyWheelVisualController =
                GetComponentInChildren<BuggyWheelVisualController>();
        }

        CacheCoreVisualFollowBase();

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

    private void LateUpdate()
    {
        UpdateCoreVisualFollow(Time.deltaTime);
    }

    public void HandleLocomotion(LocomotionCommand command, float deltaTime)
    {
        if (!IsInitialized())
            return;

        if (_legData == null || _profile == null)
            return;

        if (_context.MovementCoordinator == null)
            return;

        //// 다리 모듈의 방향은 이동 방향이 아니라 카메라 Forward 기준
        //UpdateLegYaw(command.LookDirection, command.HasMoveInput, deltaTime);

        _currentTurnInput = 0f;
        _isTurningThisFrame = false;

        // 다리 모듈에 따른 yaw 제어 다르게 하기 위해 새로운 함수 추가
        Vector3 finalMoveDirection = ResolveLegMoveDirection(command, deltaTime);

        _context.MovementCoordinator.SubmitResolvedMoveDirection(
            finalMoveDirection
        );

        UpdateMovement(
            finalMoveDirection,
            finalMoveDirection.sqrMagnitude > 0.001f,
            deltaTime
        );

        bool hasTranslationInput = finalMoveDirection.sqrMagnitude > 0.001f;

        // 실제 이동은 카메라 기준 WASD 이동 방향
        // 위에서 만든 finalMoveDirection을 통해 결정하도록 수정
        UpdateMovement(finalMoveDirection, hasTranslationInput, deltaTime);

        _isRunningByBooster =
            _context != null &&
            _context.MovementCoordinator != null &&
            _context.MovementCoordinator.WasDashBoostingLastFrame &&
            _context.LocomotionMotor != null &&
            _context.LocomotionMotor.IsGrounded &&
            command.HasMoveInput;

        UpdateBuggyWheelVisuals(); 

        // 이동 및 회전을 Animator에 전달
        UpdateAnimation(deltaTime);
    }

    public void StopLocomotion()
    {
        _currentHorizontalVelocity = Vector3.zero;

        if (_animationDriver != null)
            _animationDriver.Stop();

        if (_context != null && _context.MovementCoordinator != null)
        {
            _context.MovementCoordinator.StopAll();
        }

        if (_buggyWheelVisualController != null)
        {
            _buggyWheelVisualController.SetMotion(0f, 0f);
        }
    }

    /// <summary>
    /// 다리 방향 기준에 맞춰 회전
    /// </summary>
    /// <param name="moveDirection"></param>
    /// <param name="deltaTime"></param>
    private bool RotateLegYawTowards(Vector3 moveDirection, float deltaTime)
    {
        if (_context.LegYawRoot == null)
            return false;

        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude < 0.001f)
            return false;

        Vector3 currentForward = _context.LegYawRoot.forward;
        currentForward.y = 0f;

        if (currentForward.sqrMagnitude < 0.001f)
            return false;

        float signedAngle = Vector3.SignedAngle(
            currentForward.normalized,
            moveDirection.normalized,
            Vector3.up
        );

        if (Mathf.Abs(signedAngle) > 0.5f)
        {
            _currentTurnInput = Mathf.Clamp(signedAngle / 90f, -1f, 1f);
            _isTurningThisFrame = true;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(moveDirection.normalized, Vector3.up) *
            Quaternion.Euler(0f, _legForwardYawOffset, 0f);

        float finalLegYawSpeed =
            _baseLegYawSpeed * _profile.TurnSpeedMultiplier;

        _context.LegYawRoot.rotation = Quaternion.RotateTowards(
            _context.LegYawRoot.rotation,
            targetRotation,
            finalLegYawSpeed * deltaTime
        );

        return Mathf.Abs(signedAngle) > 0.5f;
    }

    /// <summary>
    /// 프로필 기준으로 어떤 YawControl모드냐에 따라 차체 회전을 다르게 처리
    /// </summary>
    /// <param name="command"></param>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    private Vector3 ResolveLegMoveDirection(LocomotionCommand command, float deltaTime)
    {
        if (_profile == null)
            return command.MoveDirection;

        switch (_profile.YawControlMode)
        {
            case LegYawControlMode.CameraThresholdFollow:
                if (command.HasMoveInput)
                {
                    // Spider / Humanoid:
                    // 이동 중에는 이동 방향이 아니라 바라보는 방향을 따라감
                    // 그래야 전진/후진/좌우 애니메이션이 제대로 의미를 가짐
                    _isCameraFollowTurning = false;
                    RotateLegYawTowards(command.LookDirection, deltaTime);
                }
                else
                {
                    // 정지 중에는 threshold를 넘었을 때만 따라 회전
                    UpdateCameraThresholdYaw(command.LookDirection, deltaTime);
                }
                break;

            case LegYawControlMode.MoveDirectionFollow:
                if (command.HasMoveInput)
                {
                    RotateLegYawTowards(command.MoveDirection, deltaTime);
                }
                break;

            case LegYawControlMode.MoveDirectionWithBackward:
                UpdateMoveDirectionWithBackwardYaw(command, deltaTime);
                break;

            case LegYawControlMode.VehicleSteering:
                UpdateVehicleSteeringYaw(command, deltaTime);
                break;

            case LegYawControlMode.CarSteering:
                UpdateCarSteering(command, deltaTime);
                break;

            case LegYawControlMode.None:
                break;
        }

        switch (_profile.MoveControlMode)
        {
            case LegMoveControlMode.Omnidirectional:
                return command.MoveDirection;

            case LegMoveControlMode.ForwardOnly:
                return GetForwardOnlyMoveDirection(command);

            case LegMoveControlMode.ForwardBackward:
                return GetForwardBackwardMoveDirection(command);

            default:
                return command.MoveDirection;
        }
    }

    /// <summary>
    /// 카메라 회전에 따라 차체가 따라오는 legModule용
    /// </summary>
    /// <param name="lookDirection"></param>
    /// <param name="deltaTime"></param>
    private void UpdateCameraThresholdYaw(Vector3 lookDirection, float deltaTime)
    {
        if (_context.LegYawRoot == null)
            return;

        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude < 0.001f)
            return;

        Vector3 currentForward = _context.LegYawRoot.forward;
        currentForward.y = 0f;

        if (currentForward.sqrMagnitude < 0.001f)
            return;

        // 바라보는 방향과 차체의 방향의 각도 구하기
        // 각도 차이가 일정 이상 나면(threshold) 회전하도록 하기 위함
        float angle = Vector3.Angle(currentForward.normalized, lookDirection.normalized);

        if (!_isCameraFollowTurning && angle >= _profile.TurnStartAngle)
            _isCameraFollowTurning = true;

        if (_isCameraFollowTurning && angle <= _profile.TurnStopAngle)
            _isCameraFollowTurning = false;

        if (_isCameraFollowTurning)
        {
            bool rotated = RotateLegYawTowards(lookDirection, deltaTime);

            if (rotated)
                _isTurningThisFrame = true;
        }
    }

    /// <summary>
    /// 카메라와 차체의 방향이 별개인 경우 (전진 방향으로 차체 회전)
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private Vector3 GetForwardOnlyMoveDirection(LocomotionCommand command)
    {
        if (!command.HasMoveInput)
            return Vector3.zero;

        if (_context.LegYawRoot == null)
            return command.MoveDirection;

        Vector3 forward = _context.LegYawRoot.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return command.MoveDirection;

        return forward.normalized;
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

    private void UpdateAnimation(float deltaTime)
    {
        if (_animationDriver == null)
            return;

        float moveSpeedMultiplier = 1f;

        if (_isRunningByBooster && _profile != null)
        {
            moveSpeedMultiplier = _profile.DashMultiplier;
        }

        _animationDriver.SetMoveSpeedMultiplier(moveSpeedMultiplier);

        _animationDriver.SetRunning(_isRunningByBooster);

        bool isGrounded =
            _context.LocomotionMotor != null &&
            _context.LocomotionMotor.IsGrounded;

        _animationDriver.UpdateAnimation(
            _currentHorizontalVelocity,
            _context.LegYawRoot,
            isGrounded,
            _currentTurnInput,
            _isTurningThisFrame,
            deltaTime
        );
    }

    public override void OnDetached()
    {
        ResetCoreVisualFollow();
        StopLocomotion();
    }

    private void OnDisable()
    {
        ResetCoreVisualFollow();
        StopLocomotion();
    }

    private void CacheCoreVisualFollowBase()
    {
        if (!_useCoreVisualFollow)
            return;

        if (_context == null || _context.CoreYawRoot == null)
            return;

        if (_coreFollowTarget == null)
            _coreFollowTarget = FindDefaultCoreFollowTarget();

        if (_coreFollowTarget == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Core Follow Target이 없습니다.");
            return;
        }

        _coreBaseLocalPosition = _context.CoreYawRoot.localPosition;
        _coreFollowBaseLocalPosition = GetCoreFollowTargetLocalPosition();
        _hasCoreFollowBase = true;
    }

    private Transform FindDefaultCoreFollowTarget()
    {
        Transform target = transform.Find("ROOT/Top/Mount_Top");

        if (target != null)
            return target;

        target = transform.Find("ROOT/Top");

        if (target != null)
            return target;

        target = transform.Find("ROOT/Pelvis");

        return target;
    }

    private Vector3 GetCoreFollowTargetLocalPosition()
    {
        if (_coreFollowTarget == null)
            return Vector3.zero;

        if (_context == null || _context.CoreYawRoot == null)
            return Vector3.zero;

        Transform coreParent = _context.CoreYawRoot.parent;

        if (coreParent == null)
            return _coreFollowTarget.position;

        return coreParent.InverseTransformPoint(_coreFollowTarget.position);
    }

    private void UpdateCoreVisualFollow(float deltaTime)
    {
        if (!_useCoreVisualFollow)
            return;

        if (!_hasCoreFollowBase)
            return;

        if (_context == null || _context.CoreYawRoot == null)
            return;

        Vector3 currentTargetLocalPosition = GetCoreFollowTargetLocalPosition();

        Vector3 offset =
            (currentTargetLocalPosition - _coreFollowBaseLocalPosition) *
            _coreFollowWeight;

        offset = Vector3.ClampMagnitude(offset, _coreFollowMaxOffset);

        Vector3 targetLocalPosition =
            _coreBaseLocalPosition + offset;

        _context.CoreYawRoot.localPosition = Vector3.Lerp(
            _context.CoreYawRoot.localPosition,
            targetLocalPosition,
            deltaTime * _coreFollowSmooth
        );
    }

    private void ResetCoreVisualFollow()
    {
        if (!_hasCoreFollowBase)
            return;

        if (_context == null || _context.CoreYawRoot == null)
            return;

        _context.CoreYawRoot.localPosition = _coreBaseLocalPosition;
        _hasCoreFollowBase = false;
    }

    private void UpdateMoveDirectionWithBackwardYaw(LocomotionCommand command, float deltaTime)
    {
        if (!command.HasMoveInput)
        {
            UpdateCameraThresholdYaw(command.LookDirection, deltaTime);
            return;
        }

        Vector3 moveDirection = command.MoveDirection;
        Vector3 referenceForward = command.LookDirection;

        moveDirection.y = 0f;
        referenceForward.y = 0f;

        if (moveDirection.sqrMagnitude < 0.001f ||
            referenceForward.sqrMagnitude < 0.001f)
        {
            return;
        }

        moveDirection.Normalize();
        referenceForward.Normalize();

        float forwardDot = Vector3.Dot(
            referenceForward,
            moveDirection
        );

        // 정확한 좌우 이동은 Front 영역으로 취급한다.
        bool isBackward = forwardDot < -0.001f;

        Vector3 targetFacingDirection =
            isBackward
                ? -moveDirection
                : moveDirection;

        _isCameraFollowTurning = false;

        RotateLegYawTowards(
            targetFacingDirection,
            deltaTime
        );
    }

    private void UpdateVehicleSteeringYaw(
    LocomotionCommand command,
    float deltaTime)
    {
        if (!command.HasMoveInput)
            return;

        if (_context.LegYawRoot == null)
            return;

        Vector3 lookForward = command.LookDirection;
        lookForward.y = 0f;

        if (lookForward.sqrMagnitude < 0.001f)
            return;

        lookForward.Normalize();

        Vector3 lookRight =
            Vector3.Cross(Vector3.up, lookForward).normalized;

        float steeringInput =
            Vector3.Dot(command.MoveDirection, lookRight);

        if (Mathf.Abs(steeringInput) < 0.001f)
            return;

        float finalYawSpeed =
            _baseLegYawSpeed *
            _profile.TurnSpeedMultiplier;

        float yawDelta =
            steeringInput *
            finalYawSpeed *
            deltaTime;

        _context.LegYawRoot.Rotate(
            Vector3.up,
            yawDelta,
            Space.World
        );

        _currentTurnInput = steeringInput;
        _isTurningThisFrame = true;
    }

    private Vector3 GetForwardBackwardMoveDirection(
    LocomotionCommand command)
    {
        if (!command.HasMoveInput)
            return Vector3.zero;

        if (_context.LegYawRoot == null)
            return Vector3.zero;

        Vector3 lookForward = command.LookDirection;
        lookForward.y = 0f;

        if (lookForward.sqrMagnitude < 0.001f)
            return Vector3.zero;

        lookForward.Normalize();

        float forwardInput =
            Vector3.Dot(
                command.MoveDirection,
                lookForward
            );

        if (Mathf.Abs(forwardInput) < 0.001f)
            return Vector3.zero;

        Vector3 vehicleForward =
            Quaternion.Euler(
                0f,
                -_legForwardYawOffset,
                0f
            ) *
            _context.LegYawRoot.forward;

        vehicleForward.y = 0f;

        if (vehicleForward.sqrMagnitude < 0.001f)
            return Vector3.zero;

        return vehicleForward.normalized * forwardInput;
    }

    private void UpdateCarSteering(LocomotionCommand command, float deltaTime)
    {
        if (_context.LegYawRoot == null)
            return;

        Vector3 lookForward = command.LookDirection;
        lookForward.y = 0f;

        if (lookForward.sqrMagnitude < 0.001f)
            return;

        lookForward.Normalize();

        Vector3 lookRight =
            Vector3.Cross(Vector3.up, lookForward).normalized;

        Vector3 inputDirection = command.MoveDirection;
        inputDirection.y = 0f;

        float throttle = Vector3.Dot(inputDirection, lookForward);
        float steering = Vector3.Dot(inputDirection, lookRight);

        _currentTurnInput = Mathf.Clamp(steering, -1f, 1f);
        _isTurningThisFrame = Mathf.Abs(steering) > 0.01f;

        // A/D만 누르면 바퀴 애니메이션만 움직이고 차체는 회전하지 않는다.
        if (Mathf.Abs(throttle) < 0.01f ||
            Mathf.Abs(steering) < 0.01f)
        {
            return;
        }

        float speedRatio = Mathf.Clamp01(
            _currentHorizontalVelocity.magnitude /
            Mathf.Max(_legData.MoveSpeed, 0.01f)
        );

        // 후진 중에는 조향에 따른 차체 회전 방향이 반대가 된다.
        float yawDirection =
            steering * Mathf.Sign(throttle);

        float yawAmount =
            yawDirection *
            _baseCarYawSpeed *
            _profile.TurnSpeedMultiplier *
            speedRatio *
            deltaTime;

        _context.LegYawRoot.rotation =
            Quaternion.AngleAxis(yawAmount, Vector3.up) *
            _context.LegYawRoot.rotation;
    }

    private void UpdateBuggyWheelVisuals()
    {
        if (_buggyWheelVisualController == null)
            return;

        Transform yawRoot =
            _context != null
                ? _context.LegYawRoot
                : null;

        if (yawRoot == null)
            return;

        Vector3 forward = yawRoot.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return;

        Vector3 velocity = _currentHorizontalVelocity;

        // Motor의 값에는 부스터로 추가된 속도도 포함된다.
        if (_context.LocomotionMotor != null)
        {
            velocity =
                _context.LocomotionMotor.HorizontalVelocity;
        }

        velocity.y = 0f;

        float signedSpeed = Vector3.Dot(
            velocity,
            forward.normalized
        );

        _buggyWheelVisualController.SetMotion(
            signedSpeed,
            _currentTurnInput
        );
    }
}
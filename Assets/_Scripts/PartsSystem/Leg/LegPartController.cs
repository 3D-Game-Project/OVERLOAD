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

        // 실제 이동은 카메라 기준 WASD 이동 방향
        // 위에서 만든 finalMoveDirection을 통해 결정하도록 수정
        UpdateMovement(finalMoveDirection, command.HasMoveInput, deltaTime);

        _isRunningByBooster =
            _context != null &&
            _context.MovementCoordinator != null &&
            _context.MovementCoordinator.WasDashBoostingLastFrame &&
            _context.LocomotionMotor != null &&
            _context.LocomotionMotor.IsGrounded &&
            command.HasMoveInput;

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

            case LegYawControlMode.None:
                break;
        }

        switch (_profile.MoveControlMode)
        {
            case LegMoveControlMode.Omnidirectional:
                return command.MoveDirection;

            case LegMoveControlMode.ForwardOnly:
                return GetForwardOnlyMoveDirection(command);

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
}
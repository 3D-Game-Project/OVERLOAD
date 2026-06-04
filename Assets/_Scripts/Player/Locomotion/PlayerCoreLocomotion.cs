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

    // inputhandler의 moveinput을 받아오기 (Vector2)
    // 매 프레임 이 함수를 통해 모든 움직임 관련함수에 일괄적으로 적용
    private Vector2 ReadMoveInput()
    {
        if (_playerInput == null)
            return Vector2.zero;

        // 인풋 받아오기
        Vector2 input = _playerInput.MoveInput;

        // 패드를 사용하는 걸 대비한 normalize 및 데드존 설정
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        if (Mathf.Abs(input.x) < _inputDeadZone)
            input.x = 0f;

        if (Mathf.Abs(input.y) < _inputDeadZone)
            input.y = 0f;

        return input;
    }

    // 지면 감지 후 매 순간 업데이트
    private void UpdateGroundInfo()
    {
        // 초기값 세팅
        _hasGroundBelow = false;
        _heightFromGround = Mathf.Infinity;

        // 만약 땅이 판정된 경우 groundInfo에서 거리값 받아옴
        // 코어가 공중에 떠있는 정도 확인하기 위함
        if (_groundSensor != null && _groundSensor.TryGetGround(out GroundInfo groundInfo))
        {
            _hasGroundBelow = true;
            _heightFromGround = groundInfo.Distance;
            return;
        }

        // 센서가 잠깐 바닥을 못 잡아도 CharacterController가 grounded면 바닥으로 인정 (오류 방지용)
        if (_motor != null && _motor.IsGrounded)
        {
            _hasGroundBelow = true;
            _heightFromGround = 0f;
        }
    }

    // 코어에 달린 작은 부스터 상태 업데이트용
    // 코어 출력, 높이 고려
    private void UpdateBoosterState()
    {
        if (_playerInput == null)
        {
            _isBoosterActive = false;
            return;
        }

        // 현재 출력이 남아있고, 최고 높이에 도달하지 않으면
        bool hasEnoughOutput = _currentOutput > _minOutputToUse;
        bool isBelowMaxHeight = _hasGroundBelow && _heightFromGround < _maxHeightFromGround;

        // 인풋이 있고 나머지 조건이 참이면 부스터 사용중
        // _isOutputLocked -> 쿨타임 중일때 부스터 잠시 잠금
        _isBoosterActive =
            _playerInput.IsJumpPressed &&
            !_isOutputLocked &&
            hasEnoughOutput &&
            isBelowMaxHeight;
    }

    // 출력 상태 업데이트
    // 쿨타임 및 감소 및 회복 상태 업데이트
    private void UpdateOutput()
    {
        // 부스터를 다 사용했을 때 쿨타임 관리용
        // 다 사용하자마자 부스터 사용이 가능한 걸 방지하기 위해 쿨타임으로 시간 조절
        // 쿨타임 종료시 다시 false로 전환해서 부스터 사용 가능하도록
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

        // 사용가능한 경우
        if (_isBoosterActive)
        {
            // 시간당 출력을 감소시킴
            // 0이 되면 종료
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

        // 사용가능하지도 않고 쿨타임 중이 아니며 부스터가 완충 상태가 아닐때는 재충전
        // drain하는 방식과 동일
        if (_currentOutput < _maxOutput)
        {
            _currentOutput += _outputRecoverPerSec * Time.deltaTime;
            _currentOutput = Mathf.Min(_currentOutput, _maxOutput);
        }
    }

    // 코어 방향전환용
    private void UpdateCoreRotation(bool hasMoveInput)
    {
        // 마우스만 돌릴 때는 코어가 회전하지 않음
        // WASD 입력이 있을 때만 카메라 방향으로 천천히 회전
        if (!hasMoveInput)
            return;

        // 카메라 기준 앞방향 확인
        Vector3 cameraForward = GetCameraForwardOnPlane();

        if (cameraForward.sqrMagnitude < 0.001f)
            return;

        // 회전해야하는 정도 구한 뒤 현재 위치에서 목표지점까지 천천히 회전
        // rotationSpeed를 통해 속도 조절
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            _moveRotationSpeed * Time.deltaTime
        );
    }

    // 카메라 기준 방향 확인용
    // 어딜 바라보고 있는지 XZ 평면 기준 방향으로 바꾸기
    // 이 방향을 기준으로 코어의 머리를 돌림 (에임 겸)
    private Vector3 GetCameraForwardOnPlane()
    {
        if (_cameraTransform == null)
            return transform.forward;

        // 카메라 기준 forward로 설정
        Vector3 forward = _cameraTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return transform.forward;

        // 정규화 시켜서 return
        return forward.normalized;
    }

    // 코어의 xz 평면 움직임 다루는 함수
    // 속도를 계산후 중심에 전달
    // 가감속 정도를 통해 속도 업데이트 후 속도 추가가 아닌 set을 통해 속도 업데이트
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

            // 방향과 속도를 곱해서 최종 속도 계산
            targetVelocity = moveDirection * moveSpeed;
        }

        // 인풋이 있으면 가속, 없으면 감속
        float moveRate = hasMoveInput
            ? _acceleration
            : _decceleration;

        // 최종적인 현재 속도를 계산
        // 이때 가감속정도를 통해 목표 속도에 도달
        _currentHorizontalVelocity = Vector3.MoveTowards(
            _currentHorizontalVelocity,
            targetVelocity,
            moveRate * Time.deltaTime
        );

        // 중심 제어 함수로 속도를 넘기기
        // set을 통해 수평 속도를 수정
        _motor.SetHorizontalVelocity(_currentHorizontalVelocity);
    }

    // 코어의 y축 움직임 다루는 함수 -> 부스터로 수직이동
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

            // 중심에 수직 속도 전달
            _motor.SetVerticalVelocity(dampedVelocity);
        }
    }

    // 높이 제한용
    // 남아있는 거리를 계산해서 부드럽게 도달하도록 -> 고점에 갈수록 천천히
    private float GetHeightLimitFactor()
    {
        if (!_hasGroundBelow)
            return 0f;

        // 남은 거리 계산
        // _heightFromGround는 groundSensor에서 가져온 공중에 있는 정도
        float remainingHeight = _maxHeightFromGround - _heightFromGround;

        if (remainingHeight <= 0f)
            return 0f;

        if (_heightSlowdownRange <= 0f)
            return 1f;

        float factor = Mathf.Clamp01(remainingHeight / _heightSlowdownRange);

        // 부드러운 감속 곡선
        return factor * factor;
    }

    // 움직일때 비주얼적으로 코어가 기우게 하기 위함
    private void UpdateVisualTilt(Vector2 moveInput, bool hasMoveInput)
    {
        if (_visualRoot == null)
            return;

        // 인풋을 통해 기우는 거 확인 -> 없으면 0
        float targetPitch = hasMoveInput ? moveInput.y * _tiltAmount : 0f;
        float targetRoll = hasMoveInput ? -moveInput.x * _tiltAmount : 0f;

        // 기우는 정도를 저장
        Quaternion targetRotation = Quaternion.Euler(
            targetPitch,
            0f,
            targetRoll
        );

        // 목표지점까지 천천히 기울기
        _visualRoot.localRotation = Quaternion.Slerp(
            _visualRoot.localRotation,
            targetRotation,
            _tiltSmooth * Time.deltaTime
        );
    }
}
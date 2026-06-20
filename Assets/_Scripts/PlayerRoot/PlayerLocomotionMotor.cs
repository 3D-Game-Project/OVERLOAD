using UnityEngine;

// 움직임의 중심이 되는 곳
// 다리 모듈이 변경되는 등, 이동 속도가 변경되어도 중앙에서 제어할 수 있도록 함
[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotionMotor : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private GroundSensor _groundSensor;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _groundedForce = -2f;
    [SerializeField] private float _maxFallSpeed = -25f;

    [Header("Ground Snap")]
    [SerializeField] private float _groundSnapDistance = 0.3f;
    [SerializeField] private float _groundSnapContactOffset = 0.02f;

    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;

    private bool _ignoreGroundSnapThisFrame;
    private bool _isGrounded;

    // 부스터, 다리 파츠 등이 한 프레임 동안 요청하는 값
    private float _horizontalSpeedMultiplier = 1f;
    private float _requestedMaxFallSpeed;

    public bool IsGrounded => _isGrounded;
    public Vector3 HorizontalVelocity => _horizontalVelocity;
    public float VerticalVelocity => _verticalVelocity;

    private void Awake()
    {
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();

        if (_groundSensor == null)
            _groundSensor = GetComponent<GroundSensor>();

        _requestedMaxFallSpeed = _maxFallSpeed;
    }

    private void LateUpdate()
    {
        if (_characterController == null || !_characterController.enabled || !_characterController.gameObject.activeInHierarchy) return;

        RefreshGroundedState();

        ApplyGravity();
        ApplyFallSpeedLimit();

        Vector3 finalHorizontalVelocity = _horizontalVelocity * _horizontalSpeedMultiplier;
        Vector3 finalVelocity = finalHorizontalVelocity + Vector3.up * _verticalVelocity;
        Vector3 displacement = finalVelocity * Time.deltaTime;

        if (!_characterController.isGrounded && TryGetSnapGround(out GroundInfo groundInfo))
        {
            float snapDistance = Mathf.Min(groundInfo.Distance + _groundSnapContactOffset, _groundSnapDistance);

            displacement.y = Mathf.Min(displacement.y, -snapDistance);
        }

        _characterController.Move(displacement);

        RefreshGroundedState();
        ResetFrameRequests();
    }

    // xz 평면 기준 전후좌우의 속도 제어 함수
    // 초기화 및 가감속
    // 외부에서 이 public 함수들을 통해 접근해서 속도를 넘겨주면 여기 update에서 그 속도를 적용
    // 때문에 직접적으로 움직임은 여기서만 다룸 (나머지는 다 속도만 제공)
    public void SetHorizontalVelocity(Vector3 velocity)
    {
        velocity.y = 0f;
        _horizontalVelocity = velocity;
    }

    public void AddHorizontalVelocity(Vector3 velocity)
    {
        velocity.y = 0f;
        _horizontalVelocity += velocity;
    }

    public void SetVerticalVelocity(float velocity)
    {
        _verticalVelocity = velocity;
    }

    public void AddVerticalVelocity(float velocity)
    {
        _verticalVelocity += velocity;
    }

    // 부스터 가속용
    // 한 프레임 동안 수평 속도 배율을 요청함
    public void RequestHorizontalSpeedMultiplier(float multiplier)
    {
        _horizontalSpeedMultiplier = Mathf.Max(_horizontalSpeedMultiplier, multiplier);
    }

    // 점프용
    // 수직 속도를 위쪽으로 설정하고, 그 프레임에는 바닥 보정 중력을 무시함
    public void ApplyJump(float jumpVelocity)
    {
        _verticalVelocity = jumpVelocity;
        IgnoreGroundSnapThisFrame();
    }

    // 글라이드용
    // 예: -25까지 떨어지던 것을 -2까지만 떨어지게 제한
    public void RequestFallSpeedLimit(float maxFallSpeed)
    {
        _requestedMaxFallSpeed = Mathf.Max(_requestedMaxFallSpeed, maxFallSpeed);
    }

    // 잠시 중력 해제를 위한 함수
    public void IgnoreGroundSnapThisFrame()
    {
        _ignoreGroundSnapThisFrame = true;
    }

    // 바닥에 붙이기 위한 중력 설정
    // 바닥에 붙어있기 위함 (중력만 있으면 험지에서 덜컹거릴 수 있어서)
    // ignoreGroundSnapThisFrame을 이용해서 공중에 뜰 때는 중력 잠시 취소
    private void ApplyGravity()
    {
        if (_isGrounded && _verticalVelocity < 0f && !_ignoreGroundSnapThisFrame)
        {
            _verticalVelocity = _groundedForce;
            return;
        }

        // 수직 속력에 중력 변수에 해당하는 만큼 눌러주기
        _verticalVelocity += _gravity * Time.deltaTime;

        // 기본 최대 낙하 속도 조절
        if (_verticalVelocity < _maxFallSpeed)
            _verticalVelocity = _maxFallSpeed;
    }

    private void ApplyFallSpeedLimit()
    {
        if (_verticalVelocity < _requestedMaxFallSpeed)
            _verticalVelocity = _requestedMaxFallSpeed;
    }

    private void ResetFrameRequests()
    {
        _horizontalSpeedMultiplier = 1f;
        _requestedMaxFallSpeed = _maxFallSpeed;
        _ignoreGroundSnapThisFrame = false;
    }

    private bool TryGetSnapGround(out GroundInfo groundInfo)
    {
        groundInfo = default;

        if (_groundSensor == null)
            return false;

        if (_ignoreGroundSnapThisFrame)
            return false;

        if (_verticalVelocity > 0f)
            return false;

        if (!_groundSensor.TryGetGround(out groundInfo))
            return false;

        if (groundInfo.Distance > _groundSnapDistance)
            return false;

        float groundAngle = Vector3.Angle(groundInfo.Normal, Vector3.up);

        return groundAngle <= _characterController.slopeLimit;
    }

    private void RefreshGroundedState()
    {
        if (_ignoreGroundSnapThisFrame)
        {
            _isGrounded = false;
            return;
        }

        _isGrounded = _characterController.isGrounded || TryGetSnapGround(out _);
    }
}
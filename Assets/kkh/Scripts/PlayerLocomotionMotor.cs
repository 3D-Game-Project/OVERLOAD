using UnityEngine;

// 움직임의 중심이 되는 곳
// 다리 모듈이 변경되는 등, 이동 속도가 변경되어도 중앙에서 제어할 수 있도록 함
[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotionMotor : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _groundedForce = -2f;
    [SerializeField] private float _maxFallSpeed = -25f;

    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;

    private bool _ignoreGroundSnapThisFrame;

    public bool IsGrounded => _characterController.isGrounded;
    public Vector3 HorizontalVelocity => _horizontalVelocity;
    public float VerticalVelocity => _verticalVelocity;

    private void Awake()
    {
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();
    }

    private void LateUpdate()
    {
        ApplyGravity();

        Vector3 finalVelocity = _horizontalVelocity + Vector3.up * _verticalVelocity;
        _characterController.Move(finalVelocity * Time.deltaTime);

        _ignoreGroundSnapThisFrame = false;
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

    // 잠시 중력 해제를 위한 함수
    public void IgnoreGroundSnapThisFrame()
    {
        _ignoreGroundSnapThisFrame = true;
    }

    // 바닥에 붙이기 위한 중력 설정
    // 바닥에 붙어있기 위함 (중력만있으면 험지에서 덜컹거릴수 있어서)
    // ignoreGroundSnapThisFrame을 이용해서 공중에 뜰때는 중력 잠시 취소
    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f && !_ignoreGroundSnapThisFrame)
        {
            _verticalVelocity = _groundedForce;
            return;
        }

        // 수직 속력에 중력 변수에 해당하는 만큼 눌러주기
        _verticalVelocity += _gravity * Time.deltaTime;

        // 최대 낙하 속도 조절
        if (_verticalVelocity < _maxFallSpeed)
            _verticalVelocity = _maxFallSpeed;
    }
}
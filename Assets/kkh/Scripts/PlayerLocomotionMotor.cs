using UnityEngine;

// 움직임을 총괄하는 부분
// 코어에 붙는 다리모듈에 따라 속도에 차이가 생기기 때문에 여기에서 속도를 결정
[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotionMotor : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _groundedForce = -2f;

    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;

    public bool IsGrounded => _characterController.isGrounded;
    public Vector3 HorizontalVelocity => _horizontalVelocity;
    public float VerticalVelocity => _verticalVelocity;

    private void Awake()
    {
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 중력 및 속도를 지속적으로 업데이트
        ApplyGravity();

        // 외부에서 수평과 수직 속도를 받아와 여기서 최종적으로 조합후 적용
        Vector3 finalVelocity = _horizontalVelocity + Vector3.up * _verticalVelocity;
        _characterController.Move(finalVelocity * Time.deltaTime);
    }

    // 이 아래 함수들은 모두 속도를 업데이트 해줌
    // 수평, 수직의 속도들을 추가 또는 초기화시킴
    // 조건에 따라 해당 함수들을 불러와서 속도를 업데이트 -> 이 코드에서 최종 움직임 적용
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

    //
    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity <0f)
        {
            _verticalVelocity = _groundedForce;
            return;
        }

        _verticalVelocity += _gravity * Time.deltaTime;
    }
}

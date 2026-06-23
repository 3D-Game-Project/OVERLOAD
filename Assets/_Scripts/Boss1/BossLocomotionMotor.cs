using UnityEngine;

public class BossLocomotionMotor : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;

    [Header("Gravity (중력 설정)")]
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _groundedForce = -2f;
    [SerializeField] private float _maxFallSpeed = -25f;

    private float _verticalVelocity;
    private Vector3 _currentMoveVelocity;

    private Vector3 _lookTarget;
    private bool _hasLookTarget = false;

    private void Awake()
    {
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (_characterController == null || !_characterController.enabled) return;

        ApplyGravity();

        Vector3 finalVelocity = new Vector3(_currentMoveVelocity.x, _verticalVelocity, _currentMoveVelocity.z);
        _characterController.Move(finalVelocity * Time.deltaTime);

        if (_hasLookTarget)
        {
            Vector3 lookDir = _lookTarget - transform.position;
            lookDir.y = 0f; 

            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        else if (_currentMoveVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 lookDir = new Vector3(_currentMoveVelocity.x, 0f, _currentMoveVelocity.z);
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = _groundedForce;
            return;
        }

        _verticalVelocity += _gravity * Time.deltaTime;

        if (_verticalVelocity < _maxFallSpeed)
        {
            _verticalVelocity = _maxFallSpeed;
        }
    }

    /// <summary>
    /// 컨트롤러가 이 모터에게 이동 방향과 속도를 주입하는 함수입니다.
    /// </summary>
    public void SetMoveVelocity(Vector3 velocity)
    {
        _currentMoveVelocity = velocity;
    }

    public void SetLookTarget(Vector3 targetPosition)
    {
        _lookTarget = targetPosition;
        _hasLookTarget = true;
    }

    public void ClearLookTarget()
    {
        _hasLookTarget = false;
    }
}
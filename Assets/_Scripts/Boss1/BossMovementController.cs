using UnityEngine;

public class BossMovementController : MonoBehaviour
{
    private BossLocomotionMotor _motor;

    [Header("이동 속도 설정")]
    [SerializeField] private float _baseSpeed = 10f;
    [SerializeField] private float _acceleration = 15f; 
    [SerializeField] private float _deceleration = 20f; 

    private float _currentSpeed;

    private Vector3 _targetVelocity;  
    private Vector3 _currentVelocity; 

    private void Awake()
    {
        if (_motor == null) _motor = GetComponent<BossLocomotionMotor>();
        _currentSpeed = _baseSpeed;
    }

    private void Update()
    {
        if (_motor == null) return;

        
        float speedChangeRate = _targetVelocity.sqrMagnitude > 0.01f ? _acceleration : _deceleration;

        _currentVelocity = Vector3.MoveTowards(_currentVelocity, _targetVelocity, speedChangeRate * Time.deltaTime);

        _motor.SetMoveVelocity(_currentVelocity);
    }

    /// <summary>
    /// 플레이어의 위치를 목표로 처리하여 방향 계산 (목표 속도만 설정)
    /// </summary>
    public void MoveToTarget(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position);
        direction.y = 0f;

        _targetVelocity = direction.normalized * _currentSpeed;
    }

    /// <summary>
    /// 보스의 움직임을 부드럽게 중지시키기
    /// </summary>
    public void StopMovement()
    {
        _targetVelocity = Vector3.zero;
    }

    /// <summary>
    /// 보스의 돌진 패턴 등에 대하여 일시적으로 속도를 덮어씀
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        _currentSpeed = newSpeed;
    }

    /// <summary>
    /// 원래 기본 속도로 복구
    /// </summary>
    public void ResetSpeed()
    {
        _currentSpeed = _baseSpeed;
    }

    // 모터의 회전방향 변경
    public void LookAtTarget(Vector3 targetPosition)
    {
        if (_motor != null) _motor.SetLookTarget(targetPosition);
    }

    public void ClearLookTarget()
    {
        if (_motor != null) _motor.ClearLookTarget();
    }
}
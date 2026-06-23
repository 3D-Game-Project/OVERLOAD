using UnityEngine;

public class BossMovementController : MonoBehaviour
{
    private BossLocomotionMotor _motor;

    [Header("이동 속도 설정")]
    [SerializeField] private float _baseSpeed = 10f;
    private float _currentSpeed;

    private void Awake()
    {
        if(_motor == null) _motor = GetComponent<BossLocomotionMotor>();
        _currentSpeed = _baseSpeed;
    }

    /// <summary>
    /// 플레이어의 위치를 목표로 처리하여 방향 계산
    /// </summary>
    public void MoveToTarget(Vector3 destination)
    {
        if (_motor == null) return;

        Vector3 direction = (destination - transform.position);
        direction.y = 0f;

        Vector3 velocity = direction.normalized * _currentSpeed;

        _motor.SetMoveVelocity(velocity);
    }

    /// <summary>
    /// 보스의 움직임을 즉각 중지시키기
    /// </summary>
    public void StopMovement()
    {
        if (_motor != null)
        {
            _motor.SetMoveVelocity(Vector3.zero);
        }
    }

    /// <summary>
    /// 보스의 돌진패턴에 대하여 속도를 수정
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
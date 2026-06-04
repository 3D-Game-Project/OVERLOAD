using UnityEngine;
using UnityEngine.AI;

public class EnemyLocomotionBridge : MonoBehaviour
{
    private NavMeshAgent _agent;
    private PlayerLocomotionMotor _motor;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _motor = GetComponent<PlayerLocomotionMotor>();

        _agent.updatePosition = false;
        _agent.updateRotation = false;
    }

    // 다음 목적지로 이동하는데 필요한 방향과 속도 추출
    // 이후 모터에 전달하여 실제 이동 처리
    // 몬스터의 좌표 위치 실시간 동기화
    private void Update()
    {
        Vector3 navVelocity = _agent.desiredVelocity;

        _motor.SetHorizontalVelocity(navVelocity);

        _agent.nextPosition = transform.position;

        if (navVelocity.magnitude > 0.1f)
        {
            navVelocity.y = 0f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(navVelocity), Time.deltaTime * 5f);
        }
    }
}
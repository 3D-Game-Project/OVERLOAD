using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase Target Action", story: "[Self] chases [Target] at [ChaseSpeed]", category: "Action", id: "2ac2b67db2ef839134feb505071d51fb")]
public partial class ChaseTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> ChaseSpeed;

    [SerializeReference] public BlackboardVariable<float> AttackRange;

    private NavMeshAgent _agent;

    // 타겟(플레이어)의 위치를 목적지로 파악하여 추격속도 설정하고 추격실행
    protected override Status OnStart()
    {
        if (Self?.Value == null)
        {
            return Status.Failure;
        }
        if (Target?.Value == null)
        {
            return Status.Failure;
        }

        _agent = Self.Value.GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            return Status.Failure;
        }

        Debug.Log("추격 시작");

        if (ChaseSpeed != null)
        {
            _agent.speed = ChaseSpeed.Value;
        }

        return Status.Running;
    }

    // 추격도중 공격사거리 내부로 들어온다면 멈춰서 공격 준비처리
    // 추격 액션은 성공처리 하여 다음 공격 시퀀스를 처리하는데 사용
    // 공격 사거리에 들어오지 않았다면 플레이어 위치를 기반 추격 계속 진행하도록 처리
    protected override Status OnUpdate()
    {
        if (_agent == null || Target?.Value == null) return Status.Failure;

        Vector3 currentPosXZ = new Vector3(_agent.transform.position.x, 0f, _agent.transform.position.z);
        Vector3 targetPosXZ = new Vector3(Target.Value.transform.position.x, 0f, Target.Value.transform.position.z);
        float distanceToTarget = Vector3.Distance(currentPosXZ, targetPosXZ);

        if (AttackRange != null && distanceToTarget <= AttackRange.Value)
        {
            _agent.ResetPath();

            return Status.Success;
        }

        _agent.SetDestination(Target.Value.transform.position);

        return Status.Running;
    }

    
}


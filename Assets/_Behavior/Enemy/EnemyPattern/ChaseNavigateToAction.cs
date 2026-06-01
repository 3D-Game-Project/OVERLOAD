using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase Navigate To", story: "[Self] chases [TargetPosition] but aborts if lost [CanSeeTarget] or within [AttackRange]", category: "Action", id: "1eb5a94cbdaf15daa084fe5653db7074")]
public partial class ChaseNavigateToAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> TargetPosition;
    [SerializeReference] public BlackboardVariable<bool> CanSeeTarget;
    [SerializeReference] public BlackboardVariable<float> DistanceToTarget;
    [SerializeReference] public BlackboardVariable<float> AttackRange;

    private NavMeshAgent cachedAgent;

    protected override Status OnUpdate()
    {
        if (Self?.Value == null) return Status.Failure;

        if (CanSeeTarget == null || CanSeeTarget.Value == false ||
            DistanceToTarget == null || AttackRange == null || DistanceToTarget.Value <= AttackRange.Value)
        {
            if (cachedAgent != null && cachedAgent.isOnNavMesh) cachedAgent.ResetPath();
            return Status.Failure;
        }

        if (cachedAgent == null) cachedAgent = Self.Value.GetComponent<NavMeshAgent>();
        if (cachedAgent == null || !cachedAgent.isOnNavMesh) return Status.Failure;

        // 실시간 플레이어 좌표 추격
        cachedAgent.SetDestination(TargetPosition.Value);
        return Status.Running;
    }
}


using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol", story: "[Self] patrols between [PatrolPositions] at [PatrolSpeed] speed", category: "Action", id: "29a7d7b31f380c4ddc4a250ae3b5441d")]
public partial class PatrolNavigateAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<List<GameObject>> PatrolPositions;
    [SerializeReference] public BlackboardVariable<float> PatrolSpeed;

    [SerializeReference] public BlackboardVariable<int> CurrentPatrolIndex;

    private NavMeshAgent _agent;
    private float _arrivalDistance = 0.5f;

    protected override Status OnStart()
    {
        if (Self?.Value == null) return Status.Failure;

        
        _agent = Self.Value.GetComponent<NavMeshAgent>();
        if (_agent == null) return Status.Failure;

        
        if (PatrolPositions?.Value == null || PatrolPositions.Value.Count == 0)
            return Status.Failure;

        if (PatrolSpeed != null)
        {
            _agent.speed = PatrolSpeed.Value;
        }

        return Status.Running;
    }

    // PatrolPoint를 순회하면서 목적지까지 거리 계산하여 이동
    // 이후 도착 판정을 하는 거리(_arrivalDistance)내로 들어오면 다음 정찰포인트로 이동
    // EnemyLocomotionBridge를 통해 목적지까지 이동
    protected override Status OnUpdate()
    {
        
        if (_agent == null || PatrolPositions?.Value == null || PatrolPositions.Value.Count == 0)
            return Status.Failure;

        if (CurrentPatrolIndex.Value < 0 || CurrentPatrolIndex.Value >= PatrolPositions.Value.Count)
        {
            CurrentPatrolIndex.Value = 0;
        }

        GameObject targetObj = PatrolPositions.Value[CurrentPatrolIndex.Value];
        if (targetObj == null) return Status.Failure;

        Vector3 currentPosXZ = new Vector3(_agent.transform.position.x, 0f, _agent.transform.position.z);
        Vector3 targetPosXZ = new Vector3(targetObj.transform.position.x, 0f, targetObj.transform.position.z);
        
        float distanceToTarget = Vector3.Distance(currentPosXZ, targetPosXZ);

        if (distanceToTarget <= _arrivalDistance)
        {
            CurrentPatrolIndex.Value = (CurrentPatrolIndex.Value + 1) % PatrolPositions.Value.Count;
            targetObj = PatrolPositions.Value[CurrentPatrolIndex.Value];
        }

        _agent.SetDestination(targetObj.transform.position);

        return Status.Running;
    }
}

